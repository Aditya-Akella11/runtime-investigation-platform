using RuntimeInvestigation.Application.Features.Investigations;
using RuntimeInvestigation.Infrastructure.Persistence.Repositories;
using Xunit;

namespace Application.Tests;

public class InvestigationCrudTests
{
    [Fact]
    public async Task Investigation_CanBeCreatedUpdatedTransitionedAndDeleted()
    {
        var repo = new InMemoryInvestigationRepository();
        var service = new InvestigationService(repo);

        // Create
        var createResult = await service.CreateAsync(new CreateInvestigationCommand("app-1", "Payment Timeouts", "Investigate gateway delays"));
        Assert.True(createResult.IsSuccess);
        var created = createResult.Value!;
        Assert.Equal("Payment Timeouts", created.Title);
        Assert.Equal("Open", created.Status);

        // Get by ID & list
        var getResult = await service.GetByIdAsync(created.Id);
        Assert.True(getResult.IsSuccess);
        Assert.Equal(created.Id, getResult.Value!.Id);

        var list = await service.GetAllAsync();
        Assert.Single(list);

        var byApp = await service.GetByApplicationIdAsync("app-1");
        Assert.Single(byApp);

        // Start (Open -> Investigating)
        var startResult = await service.StartAsync(created.Id);
        Assert.True(startResult.IsSuccess);
        Assert.Equal("Investigating", startResult.Value!.Status);

        // Resolve (Investigating -> Resolved)
        var resolveResult = await service.ResolveAsync(created.Id);
        Assert.True(resolveResult.IsSuccess);
        Assert.Equal("Resolved", resolveResult.Value!.Status);

        // Close (Resolved -> Closed)
        var closeResult = await service.CloseAsync(created.Id);
        Assert.True(closeResult.IsSuccess);
        Assert.Equal("Closed", closeResult.Value!.Status);

        // Delete
        var deleteResult = await service.DeleteAsync(created.Id);
        Assert.True(deleteResult.IsSuccess);
        Assert.Empty(await service.GetAllAsync());
    }

    [Fact]
    public async Task Investigation_InvalidTransitions_ShouldFail()
    {
        var repo = new InMemoryInvestigationRepository();
        var service = new InvestigationService(repo);

        var createResult = await service.CreateAsync(new CreateInvestigationCommand("app-1", "Bug"));
        Assert.True(createResult.IsSuccess);

        // Open -> Closed directly is invalid
        var closeResult = await service.CloseAsync(createResult.Value!.Id);
        Assert.False(closeResult.IsSuccess);
    }
}
