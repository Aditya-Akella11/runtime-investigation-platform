using System.Linq;
using System.Threading.Tasks;
using RuntimeInvestigation.Domain.Entities;
using RuntimeInvestigation.Infrastructure.Persistence;
using RuntimeInvestigation.Infrastructure.Persistence.Repositories;
using Xunit;

namespace Domain.Tests;

public class TemplateSeederTests
{
    [Fact]
    public void GetDefaultTemplates_ReturnsThreePreBuiltTemplates()
    {
        var templates = TemplateSeeder.GetDefaultTemplates();

        Assert.Equal(3, templates.Count);

        var nPlusOne = templates.FirstOrDefault(t => t.Name == "Debug N+1 Queries");
        Assert.NotNull(nPlusOne);
        Assert.Equal("Performance", nPlusOne.Category);
        Assert.Equal(3, nPlusOne.ProbeTemplates.Count);
        Assert.Contains(nPlusOne.ProbeTemplates, p => p.Type == ProbeType.Log);
        Assert.Contains(nPlusOne.ProbeTemplates, p => p.Type == ProbeType.Metric);

        var auth = templates.FirstOrDefault(t => t.Name == "Authentication Failures");
        Assert.NotNull(auth);
        Assert.Equal("Security", auth.Category);
        Assert.Equal(2, auth.ProbeTemplates.Count);
        Assert.Contains(auth.ProbeTemplates, p => p.Condition != null && p.Condition.Contains("false"));

        var mem = templates.FirstOrDefault(t => t.Name == "Memory Leak");
        Assert.NotNull(mem);
        Assert.Equal("Diagnostics", mem.Category);
        Assert.Equal(2, mem.ProbeTemplates.Count);
        Assert.Contains(mem.ProbeTemplates, p => p.Type == ProbeType.Snapshot);
    }

    [Fact]
    public async Task SeedAsync_PopulatesEmptyRepository()
    {
        var repo = new InMemoryTemplateRepository();
        await TemplateSeeder.SeedAsync(repo);

        var list = await repo.GetAllAsync();
        Assert.Equal(3, list.Count);
    }

    [Fact]
    public async Task SeedAsync_IsIdempotent()
    {
        var repo = new InMemoryTemplateRepository();
        await TemplateSeeder.SeedAsync(repo);
        await TemplateSeeder.SeedAsync(repo);

        var list = await repo.GetAllAsync();
        Assert.Equal(3, list.Count);
    }
}
