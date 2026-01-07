namespace ShippingHub.Domain.Entities
{
    public class Webhook
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; } = null!;

        public int EventId { get; set; }
        public WebhookEvent Event { get; set; } = null!;

        public string Url { get; set; } = "";
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
