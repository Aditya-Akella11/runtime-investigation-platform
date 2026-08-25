using RuntimeInvestigation.Domain.Entities;
using RuntimeInvestigation.Infrastructure.Persistence.Repositories;
using Xunit;

namespace Infrastructure.Tests;

public class InvestigationRepositoryTests
{
    [Fact]
    public async Task CreateAsync_ShouldPersistInvestigation()
    {
        var repository = new InMemoryInvestigationRepository();
        var investigation = new Investigation("app-payment", "Payment failure investigation", "Investigating error 500");

        await repository.CreateAsync(investigation);
        var found = await repository.GetByIdAsync(investigation.Id);

        Assert.NotNull(found);
        Assert.Equal(investigation.Id, found!.Id);
        Assert.Equal("Payment failure investigation", found.Title);
        Assert.Equal("app-payment", found.ApplicationId);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateInvestigation()
    {
        var repository = new InMemoryInvestigationRepository();
        var investigation = new Investigation("app-payment", "Original Title");
        await repository.CreateAsync(investigation);

        investigation.Update("Updated Title", "Updated Description", InvestigationStatus.Investigating);
        await repository.UpdateAsync(investigation);

        var found = await repository.GetByIdAsync(investigation.Id);
        Assert.NotNull(found);
        Assert.Equal("Updated Title", found!.Title);
        Assert.Equal(InvestigationStatus.Investigating, found.Status);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveInvestigation()
    {
        var repository = new InMemoryInvestigationRepository();
        var investigation = new Investigation("app-payment", "To be deleted");
        await repository.CreateAsync(investigation);

        var deleted = await repository.DeleteAsync(investigation.Id);
        Assert.True(deleted);

        var found = await repository.GetByIdAsync(investigation.Id);
        Assert.Null(found);
    }
}
