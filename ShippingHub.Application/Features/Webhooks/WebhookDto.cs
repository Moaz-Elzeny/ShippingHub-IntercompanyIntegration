namespace ShippingHub.Application.Features.Webhooks;

public sealed record CreateWebhookRequest(int EventId, string Url);
public sealed record ToggleWebhookRequest(bool IsActive);

public sealed record WebhookDto(
    int Id,
    int EventId,
    string EventCode,
    string Url,
    bool IsActive
);
