using RuntimeInvestigation.Domain.Entities;

namespace RuntimeInvestigation.Application.Features.Applications;

public class ApplicationService
{
    private readonly IApplicationRepository _repository;

    public ApplicationService(IApplicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ApplicationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var applications = await _repository.GetAllAsync(cancellationToken);
        return applications.Select(a => new ApplicationDto(a.Id, a.Name, a.Description, a.CreatedAt)).ToList();
    }

    public async Task<ApplicationDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var application = await _repository.GetByIdAsync(id, cancellationToken);
        return application is null ? null : new ApplicationDto(application.Id, application.Name, application.Description, application.CreatedAt);
    }

    public async Task<ApplicationDto> CreateAsync(string name, string? description, CancellationToken cancellationToken = default)
    {
        var application = new ApplicationEntity(name, description);
        var created = await _repository.AddAsync(application, cancellationToken);
        return new ApplicationDto(created.Id, created.Name, created.Description, created.CreatedAt);
    }

    public async Task<ApplicationDto?> UpdateAsync(string id, string name, string? description, CancellationToken cancellationToken = default)
    {
        var application = await _repository.GetByIdAsync(id, cancellationToken);
        if (application is null) return null;
        application.Update(name, description);
        await _repository.UpdateAsync(application, cancellationToken);
        return new ApplicationDto(application.Id, application.Name, application.Description, application.CreatedAt);
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (await _repository.GetByIdAsync(id, cancellationToken) is null) return false;
        await _repository.DeleteAsync(id, cancellationToken);
        return true;
    }
}
