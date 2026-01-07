using MediatR;
using Microsoft.EntityFrameworkCore;
using ShippingHub.Application.Abstractions;
using ShippingHub.Domain.Enums;

namespace ShippingHub.Application.Features.Auth
{
    public sealed record LoginCompanyCommand(int CompanyId, string ApiKey) : IRequest<AuthResponse>
    {
        public sealed class LoginCompanyHandler(IApplicationDbContext db, IJwtTokenService tokenService)
    : IRequestHandler<LoginCompanyCommand, AuthResponse>
        {
            public async Task<AuthResponse> Handle(LoginCompanyCommand request, CancellationToken ct)
            {
                var company = await db.Companies.FirstOrDefaultAsync(x => x.Id == request.CompanyId, ct);
                if (company is null)
                    throw new UnauthorizedAccessException("Invalid credentials.");

                if (company.Status != CompanyStatus.Active)
                    throw new UnauthorizedAccessException("Company inactive or blocked.");

                if (!string.Equals(company.ApiKey, request.ApiKey))
                    throw new UnauthorizedAccessException("Invalid credentials.");

                var token = tokenService.GenerateToken(company.Id, company.Name);
                return new AuthResponse(company.Id, company.Name, token);
            }
        }
    }
}
