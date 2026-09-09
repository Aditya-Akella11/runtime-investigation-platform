using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RuntimeInvestigation.Domain.Entities;

namespace RuntimeInvestigation.Application.Features.Users;

public interface IUserRepository
{
    Task<TenantUser?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<TenantUser?> GetByEmailAsync(string tenantId, string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantUser>> GetByTenantIdAsync(string tenantId, CancellationToken cancellationToken = default);
    Task CreateAsync(TenantUser user, CancellationToken cancellationToken = default);
    Task UpdateRoleAsync(string id, UserRole role, CancellationToken cancellationToken = default);
}
