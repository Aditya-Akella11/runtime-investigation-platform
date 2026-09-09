using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RuntimeInvestigation.API.Models;

namespace RuntimeInvestigation.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtSettings _jwtSettings;

    public AuthController(JwtSettings jwtSettings)
    {
        _jwtSettings = jwtSettings;
    }

    [HttpPost("token")]
    public ActionResult<TokenResponse> Token([FromBody] TokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Username and password are required.");
        }

        // Minimal auth stub for Epic 1; replace with provider-backed auth later.
        if (!ValidateUser(request.Username, request.Password))
        {
            return Unauthorized();
        }

        var tenantId = !string.IsNullOrWhiteSpace(request.TenantId) ? request.TenantId : "default";
        var role = !string.IsNullOrWhiteSpace(request.Role)
            ? request.Role
            : (request.Username.Equals("viewer", StringComparison.OrdinalIgnoreCase)
                ? "Viewer"
                : (request.Username.Equals("investigator", StringComparison.OrdinalIgnoreCase)
                    ? "Investigator"
                    : "Admin"));

        var token = CreateToken(request.Username, tenantId, role);
        return Ok(new TokenResponse(token));
    }

    private bool ValidateUser(string username, string password)
    {
        return password == "password" && (username == "admin" || username == "investigator" || username == "viewer" || !string.IsNullOrWhiteSpace(username));
    }

    private string CreateToken(string username, string tenantId, string role)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("tenant_id", tenantId),
            new Claim(ClaimTypes.Role, role),
            new Claim("role", role)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public sealed record TokenRequest(string Username, string Password, string? TenantId = null, string? Role = null);
public sealed record TokenResponse(string AccessToken);
