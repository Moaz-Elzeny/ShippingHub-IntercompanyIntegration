using System.Net;
using FluentValidation;

namespace ShippingHub.Api.Middlewares;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "VALIDATION_ERROR",
                details = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            // غالبًا عندنا UnauthorizedAccessException بنستخدمها لــ 401/403
            // هنخليها 403 لو فيه user authenticated، وإلا 401
            context.Response.StatusCode = context.User?.Identity?.IsAuthenticated == true
                ? (int)HttpStatusCode.Forbidden
                : (int)HttpStatusCode.Unauthorized;

            await context.Response.WriteAsJsonAsync(new
            {
                error = context.Response.StatusCode == 401 ? "UNAUTHORIZED" : "FORBIDDEN",
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "NOT_FOUND",
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            // هنستخدمها للـ 409
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "CONFLICT",
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "SERVER_ERROR",
                message = "Unexpected error occurred."
            });
        }
    }
}
