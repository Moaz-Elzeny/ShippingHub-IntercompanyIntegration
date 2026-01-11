using MediatR;
using Microsoft.EntityFrameworkCore;
using ShippingHub.Application.Abstractions;

namespace ShippingHub.Application.Features.Collaborations
{
    public sealed record GetOutgoingRequestsQuery : IRequest<List<CollaborationRequestDto>>
    {
        public sealed class GetOutgoingRequestsHandler(IApplicationDbContext db, ICurrentCompany current)
    : IRequestHandler<GetOutgoingRequestsQuery, List<CollaborationRequestDto>>
        {
            public async Task<List<CollaborationRequestDto>> Handle(GetOutgoingRequestsQuery request, CancellationToken ct)
            {
                var me = current.CompanyId;
                if (me <= 0) throw new UnauthorizedAccessException();

                var q =
                    from r in db.CollaborationRequests.AsNoTracking()
                    join s in db.Companies.AsNoTracking() on r.SenderCompanyId equals s.Id
                    join rc in db.Companies.AsNoTracking() on r.ReceiverCompanyId equals rc.Id
                    where r.SenderCompanyId == me
                    orderby r.CreatedAtUtc descending
                    select new CollaborationRequestDto(
                        r.Id,
                        r.SenderCompanyId, s.Name,
                        r.ReceiverCompanyId, rc.Name,
                        r.Status,
                        r.CreatedAtUtc
                    );

                return await q.ToListAsync(ct);
            }
        }
    }
}
