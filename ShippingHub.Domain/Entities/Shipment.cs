using ShippingHub.Domain.Enums;

namespace ShippingHub.Domain.Entities
{
    public class Shipment
    {
        public int Id { get; set; }

        // Companies
        public int SenderCompanyId { get; set; }
        public Company SenderCompany { get; set; } = null!;

        public int ReceiverCompanyId { get; set; }
        public Company ReceiverCompany { get; set; } = null!;

        // Shipment Info
        public ShipmentStatus Status { get; set; } = ShipmentStatus.Created;

        // Client Info
        public string ClientName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string AddressDescription { get; set; } = null!;

        // Financial
        public decimal CashOnDelivery { get; set; }

        // Integration Payload (External / Flexible)
        public string Payload { get; set; } = "{}";

        // Audit
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
    }

}
