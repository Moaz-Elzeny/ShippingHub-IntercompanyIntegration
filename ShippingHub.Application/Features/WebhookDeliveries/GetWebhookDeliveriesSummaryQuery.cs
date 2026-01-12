using MediatR;
using Microsoft.EntityFrameworkCore;
using ShippingHub.Application.Abstractions;
using ShippingHub.Domain.Enums;

namespace ShippingHub.Application.Features.WebhookDeliveries
{
    public sealed record GetWebhookDeliveriesSummaryQuery : IRequest<WebhookDeliverySummaryDto>
    {
        public sealed class GetWebhookDeliveriesSummaryHandler(IApplicationDbContext db, ICurrentCompany current) : IRequestHandler<GetWebhookDeliveriesSummaryQuery, WebhookDeliverySummaryDto>
        {
            public async Task<WebhookDeliverySummaryDto> Handle(GetWebhookDeliveriesSummaryQuery request, CancellationToken ct)
            {
                var companyId = current.CompanyId;
                if (companyId <= 0) throw new UnauthorizedAccessException();

                var q = db.WebhookDeliveries.AsNoTracking().Where(x => x.TargetCompanyId == companyId);

                var pending = await q.CountAsync(x => x.Status == WebhookDeliveryStatus.Pending, ct);
                var success = await q.CountAsync(x => x.Status == WebhookDeliveryStatus.Success, ct);
                var failed = await q.CountAsync(x => x.Status == WebhookDeliveryStatus.Failed, ct);

                return new WebhookDeliverySummaryDto(pending, success, failed);
            }
        }
    }
}
