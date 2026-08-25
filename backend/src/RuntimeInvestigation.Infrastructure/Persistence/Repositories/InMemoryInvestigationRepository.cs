using RuntimeInvestigation.Application.Features.Investigations;
using RuntimeInvestigation.Domain.Entities;

namespace RuntimeInvestigation.Infrastructure.Persistence.Repositories;

public sealed class InMemoryInvestigationRepository : IInvestigationRepository
{
    private readonly List<Investigation> _investigations = new();

    public Task<Investigation?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var item = _investigations.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(item);
    }

    public Task<IReadOnlyList<Investigation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Investigation>>(_investigations.OrderByDescending(x => x.CreatedAt).ToList());
    }

    public Task<IReadOnlyList<Investigation>> GetByApplicationIdAsync(string applicationId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<Investigation>>(
            _investigations.Where(x => x.ApplicationId == applicationId).OrderByDescending(x => x.CreatedAt).ToList());
    }

    public Task CreateAsync(Investigation investigation, CancellationToken cancellationToken = default)
    {
        _investigations.Add(investigation);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Investigation investigation, CancellationToken cancellationToken = default)
    {
        var existing = _investigations.FirstOrDefault(x => x.Id == investigation.Id);
        if (existing is not null)
        {
            _investigations.Remove(existing);
            _investigations.Add(investigation);
        }
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = _investigations.FirstOrDefault(x => x.Id == id);
        if (existing is null)
        {
            return Task.FromResult(false);
        }
        _investigations.Remove(existing);
        return Task.FromResult(true);
    }
}
