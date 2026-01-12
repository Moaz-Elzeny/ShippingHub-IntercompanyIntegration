using ShippingHub.Domain.Enums;

namespace ShippingHub.Application.Features.Shipments;

public static class ShipmentStatusRules
{
    public static bool IsValidTransition(ShipmentStatus from, ShipmentStatus to)
    {
        if (from == to) return true;

        return (from, to) switch
        {
            (ShipmentStatus.Created, ShipmentStatus.InTransit) => true,
            (ShipmentStatus.InTransit, ShipmentStatus.Delivered) => true,
            _ => false
        };
    }
}
