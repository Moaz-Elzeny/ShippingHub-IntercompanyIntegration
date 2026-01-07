using ShippingHub.Domain.Enums;

namespace ShippingHub.Domain.Entities
{
    public class Shipment
    {
        public int Id { get; set; }

        public int SenderCompanyId { get; set; }
        public Company SenderCompany { get; set; } = null!;

        public int ReceiverCompanyId { get; set; }
        public Company ReceiverCompany { get; set; } = null!;

        public ShipmentStatus Status { get; set; } = ShipmentStatus.Created;

        // JSON payload
        public string Payload { get; set; } = "{}";

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
