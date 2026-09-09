using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RuntimeInvestigation.Application.Features.Users;
using RuntimeInvestigation.Domain.Entities;

namespace RuntimeInvestigation.Infrastructure.Persistence.Repositories;

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<string, TenantUser> _users = new();

    public Task<TenantUser?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _users.TryGetValue(id, out var user);
        return Task.FromResult(user);
    }

    public Task<TenantUser?> GetByEmailAsync(string tenantId, string email, CancellationToken cancellationToken = default)
    {
        var user = _users.Values.FirstOrDefault(u =>
            string.Equals(u.TenantId, tenantId, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task<IReadOnlyList<TenantUser>> GetByTenantIdAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var list = _users.Values
            .Where(u => string.Equals(u.TenantId, tenantId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(u => u.CreatedAt)
            .ToArray();
        return Task.FromResult<IReadOnlyList<TenantUser>>(list);
    }

    public Task CreateAsync(TenantUser user, CancellationToken cancellationToken = default)
    {
        _users[user.Id] = user;
        return Task.CompletedTask;
    }

    public Task UpdateRoleAsync(string id, UserRole role, CancellationToken cancellationToken = default)
    {
        if (_users.TryGetValue(id, out var user))
        {
            user.Role = role;
        }
        return Task.CompletedTask;
    }

    public void Clear() => _users.Clear();
}
