using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShippingHub.Domain.Enums;
using ShippingHub.Infrastructure.Persistence;

namespace ShippingHub.Infrastructure.Services.Webhooks;

public sealed class WebhookDeliveryWorker(IServiceProvider sp, ILogger<WebhookDeliveryWorker> logger)
    : BackgroundService
{
    private const int MaxAttempts = 3;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var http = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("webhooks");

                var now = DateTime.UtcNow;

                var batch = await db.WebhookDeliveries
                    .Where(x => x.Status == WebhookDeliveryStatus.Pending && x.NextRetryAtUtc <= now)
                    .OrderBy(x => x.Id)
                    .Take(50)
                    .ToListAsync(stoppingToken);

                foreach (var d in batch)
                {
                    var hook = await db.Webhooks.AsNoTracking().FirstOrDefaultAsync(w => w.Id == d.WebhookId, stoppingToken);
                    if (hook is null || !hook.IsActive)
                    {
                        d.Status = WebhookDeliveryStatus.Failed;
                        d.LastError = "Webhook not found or inactive.";
                        d.UpdatedAtUtc = now;
                        continue;
                    }

                    try
                    {
                        var body = new
                        {
                            eventCode = d.EventCode,
                            correlationId = d.CorrelationId,
                            deliveredAtUtc = now,
                            payload = d.PayloadJson
                        };

                        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                        var rawBody = System.Text.Json.JsonSerializer.Serialize(body);

                        var signature = WebhookSignature.Sign(hook.Secret, timestamp, rawBody);

                        var req = new HttpRequestMessage(HttpMethod.Post, hook.Url)
                        {
                            Content = new StringContent(rawBody, System.Text.Encoding.UTF8, "application/json")
                        };

                        req.Headers.Add("X-ShippingHub-Signature", signature);
                        req.Headers.Add("X-ShippingHub-Timestamp", timestamp);
                        req.Headers.Add("X-ShippingHub-Event", d.EventCode);
                        req.Headers.Add("X-ShippingHub-CorrelationId", d.CorrelationId.ToString());

                        var res = await http.SendAsync(req, stoppingToken);
                        if (!res.IsSuccessStatusCode)
                        {
                            var txt = await res.Content.ReadAsStringAsync(stoppingToken);
                            throw new HttpRequestException($"Status={(int)res.StatusCode} Body={txt}");
                        }

                        d.Status = WebhookDeliveryStatus.Success;
                        d.UpdatedAtUtc = now;
                    }
                    catch (Exception ex)
                    {
                        d.AttemptCount++;
                        d.LastError = ex.Message;
                        d.UpdatedAtUtc = now;

                        if (d.AttemptCount >= MaxAttempts)
                        {
                            d.Status = WebhookDeliveryStatus.Failed;
                        }
                        else
                        {
                            var delay = TimeSpan.FromSeconds(Math.Pow(2, d.AttemptCount)); // 2,4,8
                            d.NextRetryAtUtc = now.Add(delay);
                        }
                    }
                }

                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "WebhookDeliveryWorker loop error");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }
}
