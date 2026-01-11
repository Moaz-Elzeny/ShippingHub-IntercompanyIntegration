using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShippingHub.Application.Abstractions;
using ShippingHub.Infrastructure.Persistence;

namespace ShippingHub.Infrastructure.Services.Webhooks;

public sealed class WebhookDispatcher(
    IHttpClientFactory httpClientFactory,
    ApplicationDbContext db,
    ILogger<WebhookDispatcher> logger) : IWebhookDispatcher
{
    public async Task DispatchAsync(WebhookJob job, CancellationToken ct)
    {
        var hooks = await db.Webhooks.AsNoTracking()
            .Where(w => w.CompanyId == job.TargetCompanyId && w.IsActive)
            .Join(db.WebhookEvents.AsNoTracking(),
                w => w.EventId,
                e => e.Id,
                (w, e) => new { w.Url, e.EventCode })
            .Where(x => x.EventCode == job.EventCode)
            .Select(x => x.Url)
            .ToListAsync(ct);

        if (hooks.Count == 0)
        {
            logger.LogInformation("No active webhooks found for CompanyId={CompanyId}, Event={Event}, Corr={Corr}",
                job.TargetCompanyId, job.EventCode, job.CorrelationId);
            return;
        }

        var client = httpClientFactory.CreateClient("webhooks");

        // Envelope موحد للشركات المستقبلة
        var body = new
        {
            eventCode = job.EventCode,
            correlationId = job.CorrelationId,
            deliveredAtUtc = DateTime.UtcNow,
            payload = job.PayloadJson // payload JSON string
        };

        foreach (var url in hooks)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = JsonContent.Create(body)
                };

                // Headers مفيدة للـ receiver
                req.Headers.Add("X-ShippingHub-Event", job.EventCode);
                req.Headers.Add("X-ShippingHub-CorrelationId", job.CorrelationId.ToString());

                var res = await client.SendAsync(req, ct);
                if (!res.IsSuccessStatusCode)
                {
                    var text = await res.Content.ReadAsStringAsync(ct);
                    throw new HttpRequestException($"Webhook failed. Status={(int)res.StatusCode}. Body={text}");
                }

                logger.LogInformation("Webhook delivered to {Url} (CompanyId={CompanyId}, Event={Event}, Corr={Corr})",
                    url, job.TargetCompanyId, job.EventCode, job.CorrelationId);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Webhook delivery failed to {Url} (CompanyId={CompanyId}, Event={Event}, Corr={Corr})",
                    url, job.TargetCompanyId, job.EventCode, job.CorrelationId);
                throw; // خلّي الـ worker يعمل retry
            }
        }
    }
}
