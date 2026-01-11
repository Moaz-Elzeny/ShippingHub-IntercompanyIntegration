using MediatR;
using Microsoft.EntityFrameworkCore;
using ShippingHub.Application.Abstractions;
using ShippingHub.Domain.Enums;

namespace ShippingHub.Application.Features.Companies
{
    public sealed record GetAvailableCompaniesQuery : IRequest<List<AvailableCompanyDto>>
    {
        public sealed class GetAvailableCompaniesHandler(
    IApplicationDbContext db,
    ICurrentCompany currentCompany)
    : IRequestHandler<GetAvailableCompaniesQuery, List<AvailableCompanyDto>>
        {
            public async Task<List<AvailableCompanyDto>> Handle(GetAvailableCompaniesQuery request, CancellationToken ct)
            {
                var myCompanyId = currentCompany.CompanyId;

                // لو عايزة تخليه protected فقط، خلي Authorize على الكنترولر
                if (myCompanyId <= 0)
                    throw new UnauthorizedAccessException();

                // companies: Active + have at least 1 active webhook
                var query = db.Companies.AsNoTracking()
                    .Where(c => c.Status == CompanyStatus.Active)
                    .Where(c => c.Id != myCompanyId)
                    .Where(c => db.Webhooks.Any(w => w.CompanyId == c.Id && w.IsActive))
                    .Select(c => new AvailableCompanyDto(c.Id, c.Name))
                    .OrderBy(c => c.Name);

                return await query.ToListAsync(ct);
            }
        }
    }
}
