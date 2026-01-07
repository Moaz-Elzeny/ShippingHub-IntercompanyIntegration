using ShippingHub.Domain.Enums;

namespace ShippingHub.Domain.Entities
{
    public class CollaborationRequest
    {
        public int Id { get; set; }

        public int SenderCompanyId { get; set; }
        public Company SenderCompany { get; set; } = null!;

        public int ReceiverCompanyId { get; set; }
        public Company ReceiverCompany { get; set; } = null!;

        public CollaborationRequestStatus Status { get; set; } = CollaborationRequestStatus.Pending;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
