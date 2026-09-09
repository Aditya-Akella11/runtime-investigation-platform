namespace RuntimeInvestigation.Application.Common.Interfaces;

public interface ITenantContext
{
    string TenantId { get; }
    string UserId { get; }
    string Role { get; }
    void SetContext(string tenantId, string userId, string role);
}

public class TenantContext : ITenantContext
{
    public string TenantId { get; private set; } = "default";
    public string UserId { get; private set; } = "system";
    public string Role { get; private set; } = "Admin";

    public void SetContext(string tenantId, string userId, string role)
    {
        if (!string.IsNullOrWhiteSpace(tenantId)) TenantId = tenantId.Trim();
        if (!string.IsNullOrWhiteSpace(userId)) UserId = userId.Trim();
        if (!string.IsNullOrWhiteSpace(role)) Role = role.Trim();
    }
}
