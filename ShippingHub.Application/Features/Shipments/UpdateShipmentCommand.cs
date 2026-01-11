using MediatR;
using Microsoft.EntityFrameworkCore;
using ShippingHub.Application.Abstractions;
using System.Text.Json;

namespace ShippingHub.Application.Features.Shipments
{
    public sealed record UpdateShipmentCommand(int ShipmentId, string PayloadJson, Domain.Enums.ShipmentStatus Status) : IRequest<ShipmentDto>
    {
        public sealed class UpdateShipmentHandler(
            IApplicationDbContext db,
            ICurrentCompany current,
            IWebhookQueue webhookQueue)
            : IRequestHandler<UpdateShipmentCommand, ShipmentDto>
        {
            public async Task<ShipmentDto> Handle(UpdateShipmentCommand request, CancellationToken ct)
            {
                var me = current.CompanyId;
                if (me <= 0) throw new UnauthorizedAccessException();

                ValidateJson(request.PayloadJson);

                var entity = await db.Shipments.FirstOrDefaultAsync(x => x.Id == request.ShipmentId, ct);
                if (entity is null) throw new KeyNotFoundException("Shipment not found.");

                // rule: either collaborating company can update (sender or receiver)
                if (entity.SenderCompanyId != me && entity.ReceiverCompanyId != me)
                    throw new UnauthorizedAccessException("Not allowed.");

                var oldStatus = entity.Status;

                entity.Payload = request.PayloadJson;
                entity.Status = request.Status;
                entity.UpdatedAtUtc = DateTime.UtcNow;

                await db.SaveChangesAsync(ct);

                var corr = Guid.NewGuid();

                // Notify the OTHER party (if sender updated -> notify receiver, if receiver updated -> notify sender)
                var targetCompanyId = (me == entity.SenderCompanyId) ? entity.ReceiverCompanyId : entity.SenderCompanyId;

                await webhookQueue.EnqueueAsync(new WebhookJob(
                    TargetCompanyId: targetCompanyId,
                    EventCode: "SHIPMENT_UPDATED",
                    PayloadJson: BuildEventPayload(entity, corr),
                    CorrelationId: corr
                ), ct);

                if (oldStatus != entity.Status)
                {
                    await webhookQueue.EnqueueAsync(new WebhookJob(
                        TargetCompanyId: targetCompanyId,
                        EventCode: "SHIPMENT_STATUS_CHANGED",
                        PayloadJson: BuildEventPayload(entity, corr),
                        CorrelationId: corr
                    ), ct);
                }

                return new ShipmentDto(entity.Id, entity.SenderCompanyId, entity.ReceiverCompanyId, entity.Status, entity.Payload);
            }

            private static void ValidateJson(string json)
            {
                try { JsonDocument.Parse(json); }
                catch { throw new InvalidOperationException("Invalid JSON payload."); }
            }

            private static string BuildEventPayload(Domain.Entities.Shipment s, Guid corr)
            {
                return $$"""
                         {
                           "shipmentId": {{s.Id}},
                           "senderCompanyId": {{s.SenderCompanyId}},
                           "receiverCompanyId": {{s.ReceiverCompanyId}},
                           "status": "{{s.Status}}",
                           "payload": {{s.Payload}},
                           "correlationId": "{{corr}}"
                         }
                         """;
            }
        }
    }
}
