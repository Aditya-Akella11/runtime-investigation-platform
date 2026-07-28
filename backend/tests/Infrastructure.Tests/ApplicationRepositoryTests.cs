using RuntimeInvestigation.Domain.Entities;
using RuntimeInvestigation.Infrastructure.Persistence.Repositories;
using Xunit;

namespace Infrastructure.Tests;

public class ApplicationRepositoryTests
{
    [Fact]
    public async Task AddAsync_ShouldPersistApplication()
    {
        var repository = new InMemoryApplicationRepository();
        var application = new ApplicationEntity("Inventory Service", "Handles inventory");

        var created = await repository.AddAsync(application);
        var found = await repository.GetByIdAsync(created.Id);

        Assert.NotNull(found);
        Assert.Equal(created.Id, found!.Id);
        Assert.Equal("Inventory Service", found.Name);
        Assert.Equal("Handles inventory", found.Description);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReplaceApplication()
    {
        var repository = new InMemoryApplicationRepository();
        var created = await repository.AddAsync(new ApplicationEntity("Old Name", "Old description"));

        created.Update("New Name", "New description");
        await repository.UpdateAsync(created);

        var found = await repository.GetByIdAsync(created.Id);
        Assert.NotNull(found);
        Assert.Equal("New Name", found!.Name);
        Assert.Equal("New description", found.Description);
    }
}
