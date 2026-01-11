using MediatR;
using Microsoft.EntityFrameworkCore;
using ShippingHub.Application.Abstractions;
using ShippingHub.Domain.Entities;
using ShippingHub.Domain.Enums;

namespace ShippingHub.Application.Features.Collaborations
{
    public sealed record SendCollaborationRequestCommand(int ReceiverCompanyId) : IRequest<int>
    {

        public sealed class SendCollaborationRequestHandler(
            IApplicationDbContext db,
            ICurrentCompany current)
            : IRequestHandler<SendCollaborationRequestCommand, int>
        {
            public async Task<int> Handle(SendCollaborationRequestCommand request, CancellationToken ct)
            {
                var senderId = current.CompanyId;
                if (senderId <= 0) throw new UnauthorizedAccessException();

                if (request.ReceiverCompanyId == senderId)
                    throw new InvalidOperationException("Cannot collaborate with yourself.");

                // receiver must exist + active
                var receiver = await db.Companies.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.ReceiverCompanyId, ct);

                if (receiver is null)
                    throw new KeyNotFoundException("Company not found.");

                if (receiver.Status != CompanyStatus.Active)
                    throw new UnauthorizedAccessException("Company inactive or blocked.");

                // receiver must have at least 1 active webhook (available rule)
                var receiverHasWebhook = await db.Webhooks
                    .AnyAsync(w => w.CompanyId == request.ReceiverCompanyId && w.IsActive, ct);

                if (!receiverHasWebhook)
                    throw new KeyNotFoundException("Company not available.");

                // check existing request either direction
                var exists = await db.CollaborationRequests.AnyAsync(r =>
                    (r.SenderCompanyId == senderId && r.ReceiverCompanyId == request.ReceiverCompanyId) ||
                    (r.SenderCompanyId == request.ReceiverCompanyId && r.ReceiverCompanyId == senderId), ct);

                if (exists)
                    throw new InvalidOperationException("Request already exists or collaboration already approved.");

                var entity = new CollaborationRequest
                {
                    SenderCompanyId = senderId,
                    ReceiverCompanyId = request.ReceiverCompanyId,
                    Status = CollaborationRequestStatus.Pending,
                    CreatedAtUtc = DateTime.UtcNow
                };

                db.CollaborationRequests.Add(entity);
                await db.SaveChangesAsync(ct);

                return entity.Id;
            }
        }

    }
}
