using RuntimeInvestigation.Application.Features.Applications;
using RuntimeInvestigation.Domain.Entities;

namespace RuntimeInvestigation.Infrastructure.Persistence.Repositories;

public class InMemoryApplicationRepository : IApplicationRepository
{
    private readonly List<ApplicationEntity> _applications = new();

    public Task<IReadOnlyList<ApplicationEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<ApplicationEntity>>(_applications.ToList());
    }

    public Task<ApplicationEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var application = _applications.FirstOrDefault(a => a.Id == id);
        return Task.FromResult(application);
    }

    public Task<ApplicationEntity> AddAsync(ApplicationEntity application, CancellationToken cancellationToken = default)
    {
        _applications.Add(application);
        return Task.FromResult(application);
    }

    public Task UpdateAsync(ApplicationEntity application, CancellationToken cancellationToken = default)
    {
        var existing = _applications.FirstOrDefault(a => a.Id == application.Id);
        if (existing is null)
        {
            throw new KeyNotFoundException($"Application {application.Id} was not found.");
        }

        _applications.Remove(existing);
        _applications.Add(application);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = _applications.FirstOrDefault(a => a.Id == id);
        if (existing is null)
        {
            return Task.CompletedTask;
        }

        _applications.Remove(existing);
        return Task.CompletedTask;
    }
}
