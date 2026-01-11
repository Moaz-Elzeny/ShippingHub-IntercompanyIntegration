using MediatR;
using Microsoft.EntityFrameworkCore;
using ShippingHub.Application.Abstractions;
using ShippingHub.Domain.Enums;

namespace ShippingHub.Application.Features.Collaborations
{
    public sealed record ApproveRequestCommand(int RequestId) : IRequest
    {
        public sealed class ApproveRequestHandler(IApplicationDbContext db, ICurrentCompany current)
    : IRequestHandler<ApproveRequestCommand>
        {
            public async Task Handle(ApproveRequestCommand request, CancellationToken ct)
            {
                var me = current.CompanyId;
                if (me <= 0) throw new UnauthorizedAccessException();

                var entity = await db.CollaborationRequests
                    .FirstOrDefaultAsync(x => x.Id == request.RequestId, ct);

                if (entity is null) throw new KeyNotFoundException("Request not found.");

                if (entity.ReceiverCompanyId != me)
                    throw new UnauthorizedAccessException("Not allowed.");

                if (entity.Status == CollaborationRequestStatus.Approved)
                    throw new InvalidOperationException("Already approved.");

                if (entity.Status == CollaborationRequestStatus.Rejected)
                    throw new InvalidOperationException("Already rejected.");

                entity.Status = CollaborationRequestStatus.Approved;
                await db.SaveChangesAsync(ct);
            }
        }
    }
}
