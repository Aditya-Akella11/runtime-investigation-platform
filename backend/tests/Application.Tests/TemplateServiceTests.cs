using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RuntimeInvestigation.Application.Features.Investigations;
using RuntimeInvestigation.Application.Features.Probes;
using RuntimeInvestigation.Application.Features.Templates;
using RuntimeInvestigation.Domain.Entities;
using RuntimeInvestigation.Infrastructure.Persistence;
using RuntimeInvestigation.Infrastructure.Persistence.Repositories;
using RuntimeInvestigation.Shared.Contracts;
using Xunit;

namespace Application.Tests;

public class TemplateServiceTests
{
    private sealed class DummyAgentDispatcher : IAgentDispatcher
    {
        public Task<ProbeActivationAck> DispatchActivationAsync(ProbeActivationCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(new ProbeActivationAck(command.ProbeId, "Active", AgentProtocol.Version, true));

        public Task<ProbeRemovalAck> DispatchRemovalAsync(ProbeRemovalCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(new ProbeRemovalAck(command.ProbeId, "Removed", AgentProtocol.Version, true));
    }

    private static (TemplateService service, InMemoryTemplateRepository templateRepo, InMemoryInvestigationRepository invRepo, InMemoryProbeRepository probeRepo) CreateSut()
    {
        var templateRepo = new InMemoryTemplateRepository();
        var invRepo = new InMemoryInvestigationRepository();
        var probeRepo = new InMemoryProbeRepository();
        var resultRepo = new InMemoryProbeResultRepository();

        var invService = new InvestigationService(invRepo);
        var probeService = new ProbeService(probeRepo, resultRepo, new DummyAgentDispatcher());
        var templateService = new TemplateService(templateRepo, invService, probeService);

        return (templateService, templateRepo, invRepo, probeRepo);
    }

    [Fact]
    public async Task GetTemplatesAsync_ReturnsEmptyWhenNoneExist()
    {
        var (sut, _, _, _) = CreateSut();
        var templates = await sut.GetTemplatesAsync();
        Assert.Empty(templates);
    }

    [Fact]
    public async Task GetTemplatesAsync_ReturnsTemplatesWhenSeeded()
    {
        var (sut, templateRepo, _, _) = CreateSut();
        await TemplateSeeder.SeedAsync(templateRepo);

        var templates = await sut.GetTemplatesAsync();
        Assert.Equal(3, templates.Count);
        Assert.Contains(templates, t => t.Name == "Debug N+1 Queries");
        Assert.Contains(templates, t => t.Name == "Authentication Failures");
        Assert.Contains(templates, t => t.Name == "Memory Leak");
    }

    [Fact]
    public async Task GetTemplateByIdAsync_ReturnsTemplateWhenFound()
    {
        var (sut, templateRepo, _, _) = CreateSut();
        var template = new InvestigationTemplate("Custom Template", "Description", "Custom Category", new[]
        {
            new ProbeTemplate(ProbeType.Log, "MyClass", "MyMethod")
        });
        await templateRepo.CreateAsync(template);

        var result = await sut.GetTemplateByIdAsync(template.Id);
        Assert.NotNull(result);
        Assert.Equal("Custom Template", result.Name);
        Assert.Equal("Custom Category", result.Category);
        Assert.Single(result.ProbeTemplates);
    }

    [Fact]
    public async Task GetTemplateByIdAsync_ReturnsNullWhenNotFound()
    {
        var (sut, _, _, _) = CreateSut();
        var result = await sut.GetTemplateByIdAsync("non-existent-id");
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateFromTemplateAsync_ValidationFails_WhenInputsMissing()
    {
        var (sut, _, _, _) = CreateSut();

        var res1 = await sut.CreateFromTemplateAsync(new CreateFromTemplateCommand("", "app-1"));
        Assert.False(res1.IsSuccess);
        Assert.Equal("TemplateId is required.", res1.Error?.Message);

        var res2 = await sut.CreateFromTemplateAsync(new CreateFromTemplateCommand("temp-1", ""));
        Assert.False(res2.IsSuccess);
        Assert.Equal("ApplicationId is required.", res2.Error?.Message);
    }

    [Fact]
    public async Task CreateFromTemplateAsync_Fails_WhenTemplateNotFound()
    {
        var (sut, _, _, _) = CreateSut();

        var res = await sut.CreateFromTemplateAsync(new CreateFromTemplateCommand("unknown-id", "app-1"));
        Assert.False(res.IsSuccess);
        Assert.Contains("not found", res.Error?.Message);
    }

    [Fact]
    public async Task CreateFromTemplateAsync_InstantiatesInvestigationAndDeepCopiesProbes()
    {
        var (sut, templateRepo, _, probeRepo) = CreateSut();
        await TemplateSeeder.SeedAsync(templateRepo);
        var templates = await sut.GetTemplatesAsync();
        var nPlusOneTemplate = templates.First(t => t.Name == "Debug N+1 Queries");

        var result = await sut.CreateFromTemplateAsync(new CreateFromTemplateCommand(
            nPlusOneTemplate.Id,
            "order-service-app",
            "Investigate Order N+1 Queries"));

        Assert.True(result.IsSuccess);
        var instantiation = result.Value!;
        Assert.Equal("Investigate Order N+1 Queries", instantiation.Investigation.Title);
        Assert.Equal("order-service-app", instantiation.Investigation.ApplicationId);
        Assert.Equal("Draft", instantiation.Investigation.ApprovalStatus);

        // Verify probes were created in the probe repository
        Assert.Equal(3, instantiation.Probes.Count);
        var createdProbes = await probeRepo.GetByInvestigationIdAsync(instantiation.Investigation.Id);
        Assert.Equal(3, createdProbes.Count);

        Assert.Contains(createdProbes, p => p.Target.Contains("Microsoft.EntityFrameworkCore.Query.Internal.QueryCompiler") && p.Type == ProbeType.Log);
        Assert.Contains(createdProbes, p => p.Target.Contains("System.Data.Common.DbCommand") && p.Type == ProbeType.Metric);
    }
}
