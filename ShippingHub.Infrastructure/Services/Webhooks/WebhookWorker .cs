using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ShippingHub.Application.Abstractions;

namespace ShippingHub.Infrastructure.Services.Webhooks;

public sealed class WebhookWorker : BackgroundService
{
    private readonly InMemoryWebhookQueue _queue;
    private readonly IServiceProvider _sp;
    private readonly ILogger<WebhookWorker> _logger;

    private const int MaxAttempts = 3;

    public WebhookWorker(InMemoryWebhookQueue queue, IServiceProvider sp, ILogger<WebhookWorker> logger)
    {
        _queue = queue;
        _sp = sp;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            using var scope = _sp.CreateScope();
            var dispatcher = scope.ServiceProvider.GetRequiredService<IWebhookDispatcher>();

            var attempt = job.Attempt + 1;

            try
            {
                await dispatcher.DispatchAsync(job, stoppingToken);
            }
            catch (Exception ex)
            {
                if (attempt >= MaxAttempts)
                {
                    _logger.LogError(ex, "Webhook job dropped after {Attempts} attempts. CompanyId={CompanyId}, Event={Event}, Corr={Corr}",
                        attempt, job.TargetCompanyId, job.EventCode, job.CorrelationId);
                    continue;
                }

                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt)); // 2s, 4s, 8s
                _logger.LogWarning(ex, "Retrying webhook in {Delay}s (Attempt {Attempt}/{Max}). Corr={Corr}",
                    delay.TotalSeconds, attempt, MaxAttempts, job.CorrelationId);

                try { await Task.Delay(delay, stoppingToken); } catch { /* ignore */ }

                await _queue.EnqueueAsync(job with { Attempt = attempt }, stoppingToken);
            }
        }
    }
}
