using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RuntimeInvestigation.Domain.Entities;

namespace RuntimeInvestigation.Application.Features.Probes;

public interface IProbeRepository
{
    Task<RuntimeProbe?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RuntimeProbe>> GetByInvestigationIdAsync(string investigationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RuntimeProbe>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RuntimeProbe>> GetActiveExpiredAsync(DateTime utcNow, CancellationToken cancellationToken = default);
    Task CreateAsync(RuntimeProbe probe, CancellationToken cancellationToken = default);
    Task UpdateAsync(RuntimeProbe probe, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
