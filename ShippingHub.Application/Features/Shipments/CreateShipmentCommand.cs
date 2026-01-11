using MediatR;
using Microsoft.EntityFrameworkCore;
using ShippingHub.Application.Abstractions;
using ShippingHub.Domain.Entities;
using ShippingHub.Domain.Enums;
using System.Text.Json;

namespace ShippingHub.Application.Features.Shipments
{
    public sealed record CreateShipmentCommand(int ReceiverCompanyId, string PayloadJson) : IRequest<ShipmentDto>
    {
        public sealed class CreateShipmentHandler(
            IApplicationDbContext db,
            ICurrentCompany current,
            IWebhookQueue webhookQueue) : IRequestHandler<CreateShipmentCommand, ShipmentDto>
        {
            public async Task<ShipmentDto> Handle(CreateShipmentCommand request, CancellationToken ct)
            {
                var senderId = current.CompanyId;
                if (senderId <= 0) throw new UnauthorizedAccessException();

                // validate payload json
                ValidateJson(request.PayloadJson);

                // receiver exists + active
                var receiver = await db.Companies.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.ReceiverCompanyId, ct);
                if (receiver is null) throw new KeyNotFoundException("Company not found.");
                if (receiver.Status != CompanyStatus.Active) throw new UnauthorizedAccessException("Company inactive or blocked.");

                // collaboration approved (either direction)
                var approved = await db.CollaborationRequests.AsNoTracking()
                    .AnyAsync(r =>
                        ((r.SenderCompanyId == senderId && r.ReceiverCompanyId == request.ReceiverCompanyId) ||
                         (r.SenderCompanyId == request.ReceiverCompanyId && r.ReceiverCompanyId == senderId)) &&
                        r.Status == CollaborationRequestStatus.Approved, ct);

                if (!approved) throw new UnauthorizedAccessException("No active collaboration.");

                // receiver must have at least one active webhook (rule)
                var hasWebhook = await db.Webhooks.AsNoTracking()
                    .AnyAsync(w => w.CompanyId == request.ReceiverCompanyId && w.IsActive, ct);

                if (!hasWebhook) throw new UnauthorizedAccessException("Target company has no active webhook.");

                var entity = new Shipment
                {
                    SenderCompanyId = senderId,
                    ReceiverCompanyId = request.ReceiverCompanyId,
                    Status = ShipmentStatus.Created,
                    Payload = request.PayloadJson
                };

                db.Shipments.Add(entity);
                await db.SaveChangesAsync(ct);

                var corr = Guid.NewGuid();

                // Event: created
                await webhookQueue.EnqueueAsync(new WebhookJob(
                    TargetCompanyId: entity.ReceiverCompanyId,
                    EventCode: "SHIPMENT_CREATED",
                    PayloadJson: BuildEventPayload(entity, corr),
                    CorrelationId: corr
                ), ct);

                // Event: status changed (Created)
                await webhookQueue.EnqueueAsync(new WebhookJob(
                    TargetCompanyId: entity.ReceiverCompanyId,
                    EventCode: "SHIPMENT_STATUS_CHANGED",
                    PayloadJson: BuildEventPayload(entity, corr),
                    CorrelationId: corr
                ), ct);

                return new ShipmentDto(entity.Id, entity.SenderCompanyId, entity.ReceiverCompanyId, entity.Status, entity.Payload);
            }

            private static void ValidateJson(string json)
            {
                try { JsonDocument.Parse(json); }
                catch { throw new InvalidOperationException("Invalid JSON payload."); }
            }

            private static string BuildEventPayload(Shipment s, Guid corr)
            {
                // payload envelope for receiver
                return $$""" 
                          "shipmentId": {{s.Id}},
                          "senderCompanyId": {{s.SenderCompanyId}},
                          "receiverCompanyId": {{s.ReceiverCompanyId}},
                          "status": "{{s.Status}}",
                          "payload": {{s.Payload}},
                          "correlationId": "{{corr}}"
                         """;
            }
        }
    }
}
