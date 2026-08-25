using RuntimeInvestigation.Domain.Entities;

namespace RuntimeInvestigation.Application.Features.Investigations;

public interface IInvestigationRepository
{
    Task<Investigation?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Investigation>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Investigation>> GetByApplicationIdAsync(string applicationId, CancellationToken cancellationToken = default);
    Task CreateAsync(Investigation investigation, CancellationToken cancellationToken = default);
    Task UpdateAsync(Investigation investigation, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}
