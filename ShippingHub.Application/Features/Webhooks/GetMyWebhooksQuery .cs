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
                .OrderBy(x => x.Id)
                .Select(w => new WebhookDto(w.Id, w.EventId, w.Event.EventCode, w.Url, w.IsActive, null))
                .ToListAsync(ct);
        }
    }
}
