using Microsoft.AspNetCore.Http;
using ShippingHub.Application.Abstractions;

namespace ShippingHub.Infrastructure.Services.Auth;

public sealed class CurrentCompany(IHttpContextAccessor httpContextAccessor) : ICurrentCompany
{
    public int CompanyId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            var value = user?.FindFirst("companyId")?.Value;

            return int.TryParse(value, out var id) ? id : 0;
        }
    }
}
