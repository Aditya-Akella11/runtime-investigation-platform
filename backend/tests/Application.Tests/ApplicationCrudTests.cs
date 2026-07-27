using RuntimeInvestigation.Application.Features.Applications;
using RuntimeInvestigation.Infrastructure.Persistence.Repositories;
namespace Application.Tests;
public class ApplicationCrudTests
{
    [Fact]
    public async Task Application_CanBeCreatedUpdatedListedAndDeleted()
    {
        var service = new ApplicationService(new InMemoryApplicationRepository());
        var created = await service.CreateAsync("API", "Original");
        var updated = await service.UpdateAsync(created.Id, "Public API", "Updated");
        Assert.Equal("Public API", updated!.Name);
        Assert.Single(await service.GetAllAsync());
        Assert.True(await service.DeleteAsync(created.Id));
        Assert.Empty(await service.GetAllAsync());
    }
}
