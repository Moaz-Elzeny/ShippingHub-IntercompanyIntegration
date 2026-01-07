using MediatR;
using Microsoft.EntityFrameworkCore;
using ShippingHub.Application.Abstractions;

namespace ShippingHub.Application.Features.Webhooks;

public sealed record DeleteWebhookCommand(int WebhookId) : IRequest
{
    public sealed class DeleteWebhookHandler(IApplicationDbContext db, ICurrentCompany currentCompany)
    : IRequestHandler<DeleteWebhookCommand>
    {
        public async Task Handle(DeleteWebhookCommand request, CancellationToken ct)
        {
            var companyId = currentCompany.CompanyId;
            if (companyId <= 0)
                throw new UnauthorizedAccessException("UNAUTHORIZED");

            var webhook = await db.Webhooks.FirstOrDefaultAsync(x => x.Id == request.WebhookId && x.CompanyId == companyId, ct);
            if (webhook is null)
                throw new KeyNotFoundException("WEBHOOK_NOT_FOUND");

            db.Webhooks.Remove(webhook);
            await db.SaveChangesAsync(ct);
        }
    }
}
