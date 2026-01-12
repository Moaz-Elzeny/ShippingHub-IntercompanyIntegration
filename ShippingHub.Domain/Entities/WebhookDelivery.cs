using ShippingHub.Domain.Enums;

namespace ShippingHub.Domain.Entities;

public class WebhookDelivery
{
    public int Id { get; set; }

    public int TargetCompanyId { get; set; }
    public int WebhookId { get; set; } // webhook row used

    public string EventCode { get; set; } = "";
    public string PayloadJson { get; set; } = "";
    public Guid CorrelationId { get; set; }

    public int AttemptCount { get; set; }
    public WebhookDeliveryStatus Status { get; set; } = WebhookDeliveryStatus.Pending;

    public DateTime NextRetryAtUtc { get; set; } = DateTime.UtcNow;
    public string? LastError { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
