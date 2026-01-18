using ShippingHub.Domain.Enums;

namespace ShippingHub.Application.Features.Shipments;

public sealed record UpdateShipmentRequest(string PayloadJson, ShipmentStatus Status);

public sealed record ShipmentDto(
    int Id,
    int SenderCompanyId,
    int ReceiverCompanyId,
    ShipmentStatus Status,
    string PayloadJson
);
