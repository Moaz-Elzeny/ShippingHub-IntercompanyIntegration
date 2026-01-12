using MediatR;
using Microsoft.EntityFrameworkCore;
using ShippingHub.Application.Abstractions;
using ShippingHub.Domain.Enums;

namespace ShippingHub.Application.Features.WebhookDeliveries;

public sealed record GetWebhookDeliveriesQuery(
    WebhookDeliveryStatus? Status,
    DateTime? FromUtc,
    DateTime? ToUtc,
    int Page = 1,
    int PageSize = 20) : IRequest<List<WebhookDeliveryListItemDto>>
{
    public sealed class GetWebhookDeliveriesHandler(IApplicationDbContext db, ICurrentCompany current)
    : IRequestHandler<GetWebhookDeliveriesQuery, List<WebhookDeliveryListItemDto>>
    {
        public async Task<List<WebhookDeliveryListItemDto>> Handle(GetWebhookDeliveriesQuery request, CancellationToken ct)
        {
            var companyId = current.CompanyId;
            if (companyId <= 0) throw new UnauthorizedAccessException();

            var page = request.Page <= 0 ? 1 : request.Page;
            var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

            var q = db.WebhookDeliveries.AsNoTracking()
                .Where(x => x.TargetCompanyId == companyId);

            if (request.Status is not null)
                q = q.Where(x => x.Status == request.Status);

            if (request.FromUtc is not null)
                q = q.Where(x => x.CreatedAtUtc >= request.FromUtc);

            if (request.ToUtc is not null)
                q = q.Where(x => x.CreatedAtUtc <= request.ToUtc);

            return await q
                .OrderByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WebhookDeliveryListItemDto(
                    x.Id, x.WebhookId, x.EventCode, x.Status, x.AttemptCount, x.NextRetryAtUtc, x.CreatedAtUtc
                ))
                .ToListAsync(ct);
        }
    }
}