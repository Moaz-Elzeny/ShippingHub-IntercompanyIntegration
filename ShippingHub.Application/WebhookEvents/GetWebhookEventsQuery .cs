using MediatR;
using Microsoft.EntityFrameworkCore;
using ShippingHub.Application.Abstractions;

namespace ShippingHub.Application.Features.WebhookEvents;

public sealed record GetWebhookEventsQuery : IRequest<List<WebhookEventDto>>
{
    public sealed class GetWebhookEventsHandler(IApplicationDbContext db)
    : IRequestHandler<GetWebhookEventsQuery, List<WebhookEventDto>>
    {
        public async Task<List<WebhookEventDto>> Handle(GetWebhookEventsQuery request, CancellationToken ct)
            => await db.WebhookEvents
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Select(x => new WebhookEventDto(x.Id, x.EventCode, x.Description))
                .ToListAsync(ct);
    }
}