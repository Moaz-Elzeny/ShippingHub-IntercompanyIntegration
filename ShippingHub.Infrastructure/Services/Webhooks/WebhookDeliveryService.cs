using Microsoft.EntityFrameworkCore;
using ShippingHub.Application.Abstractions;
using ShippingHub.Domain.Entities;
using ShippingHub.Infrastructure.Persistence;

namespace ShippingHub.Infrastructure.Services.Webhooks;

public sealed class WebhookDeliveryService(ApplicationDbContext db) : IWebhookDeliveryService
{
    public async Task EnqueueDeliveriesAsync(int targetCompanyId, string eventCode, string payloadJson, Guid correlationId, CancellationToken ct)
    {
        var webhookIds = await db.Webhooks.AsNoTracking()
            .Where(w => w.CompanyId == targetCompanyId && w.IsActive)
            .Join(db.WebhookEvents.AsNoTracking(),
                w => w.EventId,
                e => e.Id,
                (w, e) => new { w.Id, e.EventCode })
            .Where(x => x.EventCode == eventCode)
            .Select(x => x.Id)
            .ToListAsync(ct);

        foreach (var wid in webhookIds)
        {
            db.WebhookDeliveries.Add(new WebhookDelivery
            {
                TargetCompanyId = targetCompanyId,
                WebhookId = wid,
                EventCode = eventCode,
                PayloadJson = payloadJson,
                CorrelationId = correlationId,
                NextRetryAtUtc = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync(ct);
    }
}
