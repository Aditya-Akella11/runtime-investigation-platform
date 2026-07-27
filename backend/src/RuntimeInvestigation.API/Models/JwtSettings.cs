namespace RuntimeInvestigation.API.Models;

public sealed class JwtSettings
{
    public string Issuer { get; set; } = "RuntimeInvestigation";
    public string Audience { get; set; } = "RuntimeInvestigationClient";
    public string SigningKey { get; set; } = string.Empty;
}
