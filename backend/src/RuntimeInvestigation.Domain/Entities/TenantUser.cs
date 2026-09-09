using System;

namespace RuntimeInvestigation.Domain.Entities;

public class TenantUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string TenantId { get; set; } = "default";
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Viewer;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public TenantUser() { }

    public TenantUser(string tenantId, string email, string name, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentException("TenantId is required.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));

        Id = Guid.NewGuid().ToString("N");
        TenantId = tenantId.Trim();
        Email = email.Trim().ToLowerInvariant();
        Name = name?.Trim() ?? string.Empty;
        Role = role;
        CreatedAt = DateTime.UtcNow;
    }
}
