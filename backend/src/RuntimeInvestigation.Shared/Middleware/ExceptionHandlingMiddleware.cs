using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using RuntimeInvestigation.Shared.Exceptions;
namespace RuntimeInvestigation.Shared.Middleware;
public sealed class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (Exception exception)
        {
            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = exception switch { ValidationException => (int)HttpStatusCode.BadRequest, NotFoundException => (int)HttpStatusCode.NotFound, _ => (int)HttpStatusCode.InternalServerError };
            var detail = exception is PlatformException ? exception.Message : "An unexpected error occurred.";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { status = context.Response.StatusCode, detail, traceId = context.TraceIdentifier }));
        }
    }
}
