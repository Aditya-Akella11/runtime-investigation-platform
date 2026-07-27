using RuntimeInvestigation.Domain.Entities;

namespace RuntimeInvestigation.Application.Features.Applications;

public interface IApplicationRepository
{
    Task<IReadOnlyList<ApplicationEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApplicationEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<ApplicationEntity> AddAsync(ApplicationEntity application, CancellationToken cancellationToken = default);
    Task UpdateAsync(ApplicationEntity application, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
