using Microsoft.AspNetCore.Builder;
using RuntimeInvestigation.Shared.Middleware;
namespace RuntimeInvestigation.Shared.Extensions;
public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UsePlatformExceptionHandling(this IApplicationBuilder app) => app.UseMiddleware<ExceptionHandlingMiddleware>();
}
