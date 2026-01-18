using MediatR;
using Microsoft.EntityFrameworkCore;
using ShippingHub.Application.Abstractions;
using ShippingHub.Domain.Entities;
using ShippingHub.Domain.Enums;
using System.Text.Json;

namespace ShippingHub.Application.Features.Shipments;

public sealed record CreateShipmentCommand(int ReceiverCompanyId, CreateShipmentRequestDto Dto) : IRequest<ShipmentDto>;

public sealed class CreateShipmentHandler(
    IApplicationDbContext db,
    ICurrentCompany current,
    IWebhookDeliveryService deliveryService)
    : IRequestHandler<CreateShipmentCommand, ShipmentDto>
{
    public async Task<ShipmentDto> Handle(CreateShipmentCommand request, CancellationToken ct)
    {
        var senderId = current.CompanyId;
        if (senderId <= 0) throw new UnauthorizedAccessException();

        // receiver exists + active
        var receiver = await db.Companies.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.ReceiverCompanyId, ct);

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

        // payload -> json string
        var payloadJson = SerializePayload(request.Dto.Payload);

        var shipment = new Shipment
        {
            SenderCompanyId = senderId,
            ReceiverCompanyId = request.ReceiverCompanyId,

            ClientName = request.Dto.ClientName,
            PhoneNumber = request.Dto.PhoneNumber,
            AddressDescription = request.Dto.AddressDescription,
            CashOnDelivery = request.Dto.CashOnDelivery,

            Status = request.Dto.Status ?? ShipmentStatus.Created,
            Payload = payloadJson,

            CreatedAtUtc = DateTime.UtcNow
        };

        db.Shipments.Add(shipment);
        await db.SaveChangesAsync(ct);

        // Enqueue webhook deliveries
        var corr = Guid.NewGuid();
        var eventPayload = BuildEventPayload(shipment, corr);

        await deliveryService.EnqueueDeliveriesAsync(
            targetCompanyId: shipment.ReceiverCompanyId,
            eventCode: "SHIPMENT_CREATED",
            payloadJson: eventPayload,
            correlationId: corr,
            ct);

        await deliveryService.EnqueueDeliveriesAsync(
            targetCompanyId: shipment.ReceiverCompanyId,
            eventCode: "SHIPMENT_STATUS_CHANGED",
            payloadJson: eventPayload,
            correlationId: corr,
            ct);

        return new ShipmentDto(shipment.Id, shipment.SenderCompanyId, shipment.ReceiverCompanyId, shipment.Status, shipment.Payload);
    }

    private static string SerializePayload(object? payload)
    {
        // لو null نخليها {}
        return payload is null
            ? "{}"
            : JsonSerializer.Serialize(payload);
    }

    private static string BuildEventPayload(Shipment s, Guid corr)
    {
        // ده JSON صحيح
        return $$"""
        {
          "shipmentId": {{s.Id}},
          "senderCompanyId": {{s.SenderCompanyId}},
          "receiverCompanyId": {{s.ReceiverCompanyId}},
          "status": "{{s.Status}}",
          "clientName": {{JsonSerializer.Serialize(s.ClientName)}},
          "phoneNumber": {{JsonSerializer.Serialize(s.PhoneNumber)}},
          "addressDescription": {{JsonSerializer.Serialize(s.AddressDescription)}},
          "cashOnDelivery": {{s.CashOnDelivery}},
          "payload": {{s.Payload}},
          "correlationId": "{{corr}}"
        }
        """;
    }
}
