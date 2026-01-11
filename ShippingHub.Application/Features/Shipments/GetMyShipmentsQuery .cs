using MediatR;
using Microsoft.EntityFrameworkCore;
using ShippingHub.Application.Abstractions;

namespace ShippingHub.Application.Features.Shipments;
public sealed record GetMyShipmentsQuery : IRequest<List<ShipmentDto>>
{
    public sealed class GetMyShipmentsHandler(IApplicationDbContext db, ICurrentCompany current)
    : IRequestHandler<GetMyShipmentsQuery, List<ShipmentDto>>
    {
        public async Task<List<ShipmentDto>> Handle(GetMyShipmentsQuery request, CancellationToken ct)
        {
            var me = current.CompanyId;
            if (me <= 0) throw new UnauthorizedAccessException();

            return await db.Shipments.AsNoTracking()
                .Where(x => x.SenderCompanyId == me || x.ReceiverCompanyId == me)
                .OrderByDescending(x => x.Id)
                .Select(x => new ShipmentDto(x.Id, x.SenderCompanyId, x.ReceiverCompanyId, x.Status, x.Payload))
                .ToListAsync(ct);
        }
    }
}