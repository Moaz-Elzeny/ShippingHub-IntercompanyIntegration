using MediatR;
using Microsoft.EntityFrameworkCore;
using ShippingHub.Application.Abstractions;

namespace ShippingHub.Application.Features.Webhooks;

public sealed record GetMyWebhooksQuery : IRequest<List<WebhookDto>>
{
    public sealed class GetMyWebhooksHandler(IApplicationDbContext db, ICurrentCompany currentCompany)
    : IRequestHandler<GetMyWebhooksQuery, List<WebhookDto>>
    {
        public async Task<List<WebhookDto>> Handle(GetMyWebhooksQuery request, CancellationToken ct)
        {
            var companyId = currentCompany.CompanyId;
            if (companyId <= 0)
                throw new UnauthorizedAccessException("UNAUTHORIZED");

            return await db.Webhooks
                .AsNoTracking()
                .Where(x => x.CompanyId == companyId)
                .Join(db.WebhookEvents.AsNoTracking(),
                    w => w.EventId,
                    e => e.Id,
                    (w, e) => new WebhookDto(w.Id, e.Id, e.EventCode, w.Url, w.IsActive))
                .OrderBy(x => x.Id)
                .ToListAsync(ct);
        }
    }
}
