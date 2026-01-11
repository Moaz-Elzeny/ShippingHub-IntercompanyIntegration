using MediatR;
using Microsoft.EntityFrameworkCore;
using ShippingHub.Application.Abstractions;
using ShippingHub.Domain.Entities;
using ShippingHub.Domain.Enums;
using System.Security.Cryptography;

namespace ShippingHub.Application.Features.Auth
{
    public sealed record RegisterCompanyCommand(string Name) : IRequest<AuthResponse>
    {
        public sealed class RegisterCompanyHandler(IApplicationDbContext db, IJwtTokenService tokenService) : IRequestHandler<RegisterCompanyCommand, AuthResponse>
        {
            public async Task<AuthResponse> Handle(RegisterCompanyCommand request, CancellationToken ct)
            {
                var exists = await db.Companies.AnyAsync(x => x.Name == request.Name, ct);
                if (exists)
                    throw new InvalidOperationException("Company name already exists.");

                var apiKey = GenerateApiKey();

                var company = new Company
                {
                    Name = request.Name.Trim(),
                    Status = CompanyStatus.Active,
                    ApiKey = apiKey
                };

                db.Companies.Add(company);
                await db.SaveChangesAsync(ct);

                var token = tokenService.GenerateToken(company.Id, company.Name);

                // رجّعي apiKey مرة واحدة (عادة بتتعرض مرة واحدة في UI)
                return new AuthResponse(company.Id, company.Name, token);
            }

            private static string GenerateApiKey()
            {
                // MVP فقط: API Key عشوائي
                var bytes = RandomNumberGenerator.GetBytes(32);
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
