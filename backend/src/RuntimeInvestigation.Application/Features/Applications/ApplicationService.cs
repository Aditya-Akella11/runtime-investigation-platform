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

    public async Task<ApplicationDto> CreateAsync(string name, string? description, CancellationToken cancellationToken = default)
    {
        var application = new ApplicationEntity(name, description);
        var created = await _repository.AddAsync(application, cancellationToken);
        return new ApplicationDto(created.Id, created.Name, created.Description, created.CreatedAt);
    }
}
