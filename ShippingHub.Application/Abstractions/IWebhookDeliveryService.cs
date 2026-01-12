namespace ShippingHub.Application.Abstractions;

public interface IWebhookDeliveryService
{
    Task EnqueueDeliveriesAsync(int targetCompanyId, string eventCode, string payloadJson, Guid correlationId, CancellationToken ct);
}
