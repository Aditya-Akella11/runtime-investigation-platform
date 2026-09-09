using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using RuntimeInvestigation.Application.Common.Interfaces;

namespace RuntimeInvestigation.API.Middleware;

public class TenantContextMiddleware
{
    private readonly RequestDelegate _next;

    public TenantContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var tenantIdClaim = context.User.FindFirst("tenant_id")?.Value;
        var roleClaim = context.User.FindFirst(ClaimTypes.Role)?.Value ?? context.User.FindFirst("role")?.Value ?? "Viewer";
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? "system";

        var tenantHeader = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
        var effectiveTenantId = tenantIdClaim ?? tenantHeader ?? "default";

        tenantContext.SetContext(effectiveTenantId, userId, roleClaim);

        // If the endpoint requires authorization and user is authenticated, ensure tenant_id claim exists
        var endpoint = context.GetEndpoint();
        var authorizeData = endpoint?.Metadata.GetOrderedMetadata<IAuthorizeData>();
        if (authorizeData != null && authorizeData.Any() && context.User.Identity?.IsAuthenticated == true)
        {
            if (string.IsNullOrWhiteSpace(tenantIdClaim) && string.IsNullOrWhiteSpace(tenantHeader))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "Missing tenant_id in token or header." });
                return;
            }
        }

        await _next(context);
    }
}
