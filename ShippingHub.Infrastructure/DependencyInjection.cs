using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShippingHub.Application.Abstractions;
using ShippingHub.Infrastructure.Persistence;
using ShippingHub.Infrastructure.Services.Auth;

namespace ShippingHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<ApplicationDbContext>(opt =>
            opt.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        // الصح: IApplicationDbContext يتسجل من نفس DbContext
        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        // ✅ دي الصيغة الصحيحة (مش Configure<JwtOptions>(config.GetSection...) بصيغة تانية)
        services.Configure<JwtOptions>(config.GetSection("Jwt"));

        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.AddScoped<ICurrentCompany, CurrentCompany>();

        return services;
    }
}
