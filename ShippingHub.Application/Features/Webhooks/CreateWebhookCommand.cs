using MediatR;
using Microsoft.EntityFrameworkCore;
using ShippingHub.Application.Abstractions;
using ShippingHub.Domain.Entities;

namespace ShippingHub.Application.Features.Webhooks;

public sealed record CreateWebhookCommand(int EventId, string Url) : IRequest<WebhookDto>
{
    public sealed class CreateWebhookHandler(
    IApplicationDbContext db,
    ICurrentCompany currentCompany)
    : IRequestHandler<CreateWebhookCommand, WebhookDto>
    {
        public async Task<WebhookDto> Handle(CreateWebhookCommand request, CancellationToken ct)
        {
            var companyId = currentCompany.CompanyId;
            if (companyId <= 0)
                throw new UnauthorizedAccessException("UNAUTHORIZED");

            var ev = await db.WebhookEvents.FirstOrDefaultAsync(x => x.Id == request.EventId, ct);
            if (ev is null)
                throw new KeyNotFoundException("EVENT_NOT_SUPPORTED"); // 404

            var exists = await db.Webhooks.AnyAsync(x => x.CompanyId == companyId && x.EventId == request.EventId, ct);
            if (exists)
                throw new InvalidOperationException("WEBHOOK_ALREADY_EXISTS"); // 409

            var webhook = new Webhook
            {
                CompanyId = companyId,
                EventId = request.EventId,
                Url = request.Url.Trim(),
                IsActive = true
            };

            db.Webhooks.Add(webhook);
            await db.SaveChangesAsync(ct);

            return new WebhookDto(webhook.Id, ev.Id, ev.EventCode, webhook.Url, webhook.IsActive);
        }
    }
}
