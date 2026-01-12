using MediatR;
using Microsoft.EntityFrameworkCore;
using ShippingHub.Application.Abstractions;
using ShippingHub.Domain.Enums;

namespace ShippingHub.Application.Features.WebhookDeliveries
{
    public sealed record RetryWebhookDeliveryCommand(int Id) : IRequest
    {
        public sealed class RetryWebhookDeliveryHandler(IApplicationDbContext db, ICurrentCompany current)
    : IRequestHandler<RetryWebhookDeliveryCommand>
        {
            public async Task Handle(RetryWebhookDeliveryCommand request, CancellationToken ct)
            {
                var companyId = current.CompanyId;
                if (companyId <= 0) throw new UnauthorizedAccessException();

                var d = await db.WebhookDeliveries
                    .FirstOrDefaultAsync(x => x.Id == request.Id && x.TargetCompanyId == companyId, ct);

                if (d is null) throw new KeyNotFoundException("Delivery not found.");

                // رجّعيه Pending وجدولي retry فورًا
                d.Status = WebhookDeliveryStatus.Pending;
                d.NextRetryAtUtc = DateTime.UtcNow;
                d.LastError = null;
                d.UpdatedAtUtc = DateTime.UtcNow;

                await db.SaveChangesAsync(ct);
            }
        }
    }
}
