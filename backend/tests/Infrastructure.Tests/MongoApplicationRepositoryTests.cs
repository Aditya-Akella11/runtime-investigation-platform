using Mongo2Go;
using RuntimeInvestigation.Domain.Entities;
using RuntimeInvestigation.Infrastructure.Persistence.Repositories;
using Xunit;

namespace Infrastructure.Tests;

public class MongoApplicationRepositoryTests : IDisposable
{
    private readonly MongoDbRunner _runner;
    private readonly MongoApplicationRepository _repository;

    public MongoApplicationRepositoryTests()
    {
        _runner = MongoDbRunner.Start();
        var settings = new MongoDbSettings
        {
            ConnectionString = _runner.ConnectionString,
            DatabaseName = "RuntimeInvestigationTest",
            ApplicationsCollectionName = "applications"
        };

        _repository = new MongoApplicationRepository(settings);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistApplication()
    {
        var application = new ApplicationEntity("Inventory Service", "Handles inventory");

        var created = await _repository.AddAsync(application);
        var found = await _repository.GetByIdAsync(created.Id);

        Assert.NotNull(found);
        Assert.Equal(created.Id, found!.Id);
        Assert.Equal("Inventory Service", found.Name);
        Assert.Equal("Handles inventory", found.Description);
    }

    public void Dispose()
    {
        _runner.Dispose();
    }
}
