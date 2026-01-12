using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShippingHub.Infrastructure.Persistence;

namespace ShippingHub.Api.Middlewares;

public sealed class IdempotencyMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context, ApplicationDbContext db)
    {
        if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var key))
        {
            await next(context);
            return;
        }

        var companyIdClaim = context.User.FindFirst("companyId")?.Value;
        if (!int.TryParse(companyIdClaim, out var companyId))
        {
            await next(context);
            return;
        }

        // hash request body
        context.Request.EnableBuffering();
        using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        context.Request.Body.Position = 0;

        var hash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(body))
        );

        var existing = await db.IdempotencyKeys
            .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Key == key);

        if (existing != null)
        {
            context.Response.StatusCode = existing.StatusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(existing.ResponseBody);
            return;
        }

        // capture response
        var originalBody = context.Response.Body;
        using var mem = new MemoryStream();
        context.Response.Body = mem;

        await next(context);

        mem.Position = 0;
        var responseBody = await new StreamReader(mem).ReadToEndAsync();

        db.IdempotencyKeys.Add(new Domain.Entities.IdempotencyKey
        {
            CompanyId = companyId,
            Key = key!,
            RequestHash = hash,
            ResponseBody = responseBody,
            StatusCode = context.Response.StatusCode
        });

        await db.SaveChangesAsync();

        mem.Position = 0;
        await mem.CopyToAsync(originalBody);
    }
}
