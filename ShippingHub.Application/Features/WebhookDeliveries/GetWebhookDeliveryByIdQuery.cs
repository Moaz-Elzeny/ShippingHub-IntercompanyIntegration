using MediatR;
using Microsoft.EntityFrameworkCore;
using ShippingHub.Application.Abstractions;

namespace ShippingHub.Application.Features.WebhookDeliveries
{
    public sealed record GetWebhookDeliveryByIdQuery(int Id) : IRequest<WebhookDeliveryDetailsDto>
    {
        public sealed class GetWebhookDeliveryByIdHandler(IApplicationDbContext db, ICurrentCompany current) : IRequestHandler<GetWebhookDeliveryByIdQuery, WebhookDeliveryDetailsDto>
        {
            public async Task<WebhookDeliveryDetailsDto> Handle(GetWebhookDeliveryByIdQuery request, CancellationToken ct)
            {
                var companyId = current.CompanyId;
                if (companyId <= 0) throw new UnauthorizedAccessException();

                var d = await db.WebhookDeliveries.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.Id && x.TargetCompanyId == companyId, ct);

                if (d is null) throw new KeyNotFoundException("Delivery not found.");

                return new WebhookDeliveryDetailsDto(
                    d.Id, d.TargetCompanyId, d.WebhookId, d.EventCode, d.PayloadJson, d.CorrelationId,
                    d.Status, d.AttemptCount, d.NextRetryAtUtc, d.LastError, d.CreatedAtUtc, d.UpdatedAtUtc
                );
            }
        }
    }
}
