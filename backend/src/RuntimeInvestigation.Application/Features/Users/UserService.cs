using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RuntimeInvestigation.Domain.Entities;

namespace RuntimeInvestigation.Application.Features.Users;

public class UserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<TenantUser> CreateUserAsync(string tenantId, string email, string name, UserRole role, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) throw new ArgumentException("TenantId is required.", nameof(tenantId));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));

        var existing = await _userRepository.GetByEmailAsync(tenantId, email, cancellationToken);
        if (existing != null)
        {
            throw new InvalidOperationException($"User with email '{email}' already exists in tenant '{tenantId}'.");
        }

        var user = new TenantUser(tenantId, email, name, role);
        await _userRepository.CreateAsync(user, cancellationToken);
        return user;
    }

    public async Task<IReadOnlyList<TenantUser>> GetUsersAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId)) return Array.Empty<TenantUser>();
        return await _userRepository.GetByTenantIdAsync(tenantId, cancellationToken);
    }

    public async Task<TenantUser?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return await _userRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<TenantUser?> UpdateRoleAsync(string id, UserRole newRole, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null) return null;

        await _userRepository.UpdateRoleAsync(id, newRole, cancellationToken);
        user.Role = newRole;
        return user;
    }
}
