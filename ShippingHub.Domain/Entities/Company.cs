using ShippingHub.Domain.Enums;

namespace ShippingHub.Domain.Entities
{
    public class Company
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public CompanyStatus Status { get; set; } = CompanyStatus.Inactive;

        // API Key بسيط للـ MVP (خزّنيه Hash في النسخة المتقدمة)
        public string ApiKey { get; set; } = "";

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<Webhook> Webhooks { get; set; } = new List<Webhook>();
    }
}
