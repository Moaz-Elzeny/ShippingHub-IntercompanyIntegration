namespace ShippingHub.Application.Abstractions;

public interface IWebhookDispatcher
{
    Task DispatchAsync(WebhookJob job, CancellationToken ct);
}
