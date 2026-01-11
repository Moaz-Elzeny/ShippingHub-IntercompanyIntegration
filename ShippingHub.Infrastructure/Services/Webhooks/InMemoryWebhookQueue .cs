using System.Threading.Channels;
using ShippingHub.Application.Abstractions;

namespace ShippingHub.Infrastructure.Services.Webhooks;

public sealed class InMemoryWebhookQueue : IWebhookQueue
{
    private readonly Channel<WebhookJob> _channel;

    public InMemoryWebhookQueue()
    {
        _channel = Channel.CreateUnbounded<WebhookJob>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    }

    public ChannelReader<WebhookJob> Reader => _channel.Reader;

    public ValueTask EnqueueAsync(WebhookJob job, CancellationToken ct = default)
        => _channel.Writer.WriteAsync(job, ct);
}
