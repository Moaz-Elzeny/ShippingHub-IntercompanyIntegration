namespace ShippingHub.Application.Abstractions;

public sealed record WebhookJob(
    int TargetCompanyId,
    string EventCode,
    string PayloadJson,
    Guid CorrelationId,
    int Attempt = 0
);

public interface IWebhookQueue
{
    ValueTask EnqueueAsync(WebhookJob job, CancellationToken ct = default);
}
