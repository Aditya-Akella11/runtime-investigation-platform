using RuntimeInvestigation.Application.Features.Applications;
using RuntimeInvestigation.Infrastructure.Persistence.Repositories;

namespace Application.Tests;

public class ApplicationServiceTests
{
    [Fact]
    public async Task CreateAsync_ShouldPersistApplication()
    {
        var repository = new InMemoryApplicationRepository();
        var service = new ApplicationService(repository);

        var created = await service.CreateAsync("Inventory Service", "Handles inventory");

        Assert.Equal("Inventory Service", created.Name);
        Assert.Equal("Handles inventory", created.Description);
        Assert.False(string.IsNullOrWhiteSpace(created.Id));
    }
}
