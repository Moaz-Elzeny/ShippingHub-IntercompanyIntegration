using MediatR;
using Microsoft.EntityFrameworkCore;
using ShippingHub.Application.Abstractions;

namespace ShippingHub.Application.Features.Webhooks;

public sealed record ToggleWebhookCommand(int WebhookId, bool IsActive) : IRequest
{
    public sealed class ToggleWebhookHandler(IApplicationDbContext db, ICurrentCompany currentCompany)
    : IRequestHandler<ToggleWebhookCommand>
    {
        public async Task Handle(ToggleWebhookCommand request, CancellationToken ct)
        {
            var companyId = currentCompany.CompanyId;
            if (companyId <= 0)
                throw new UnauthorizedAccessException("UNAUTHORIZED");

            var webhook = await db.Webhooks.FirstOrDefaultAsync(x => x.Id == request.WebhookId && x.CompanyId == companyId, ct);
            if (webhook is null)
                throw new KeyNotFoundException("WEBHOOK_NOT_FOUND");

            webhook.IsActive = request.IsActive;
            await db.SaveChangesAsync(ct);
        }
    }
}
