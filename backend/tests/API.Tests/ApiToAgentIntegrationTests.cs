using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RuntimeInvestigation.API.Controllers;
using RuntimeInvestigation.Application.Features.Investigations;
using RuntimeInvestigation.Application.Features.Probes;
using RuntimeInvestigation.Infrastructure.Persistence.Repositories;
using RuntimeInvestigation.Shared.Contracts;
using Xunit;

namespace API.Tests;

public class ApiToAgentIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiToAgentIntegrationTests(WebApplicationFactory<Program> factory)
    {
        var inMemoryInvestigationRepo = new InMemoryInvestigationRepository();
        var inMemoryProbeRepo = new InMemoryProbeRepository();
        var inMemoryResultRepo = new InMemoryProbeResultRepository();

        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IInvestigationRepository>(inMemoryInvestigationRepo);
                services.AddSingleton<IProbeRepository>(inMemoryProbeRepo);
                services.AddSingleton<IProbeResultRepository>(inMemoryResultRepo);
            });
        });
    }

    [Fact]
    public async Task FullProbeWorkflow_CreateAddActivateResultsDeactivate_Succeeds()
    {
        var client = _factory.CreateClient();
        var tokenResponse = await client.PostAsJsonAsync("/api/auth/token", new { Username = "admin", Password = "password" });
        tokenResponse.EnsureSuccessStatusCode();
        var token = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(token);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token!.AccessToken);

        // 1. Create Investigation
        var createInvReq = new CreateInvestigationCommand("app-123", "Payment Timeout Spikes", "Investigate timeout errors");
        var invResponse = await client.PostAsJsonAsync("/api/investigations", createInvReq);
        Assert.True(invResponse.IsSuccessStatusCode);
        var inv = await invResponse.Content.ReadFromJsonAsync<InvestigationDto>();
        Assert.NotNull(inv);

        // 2. Add Probe to Investigation
        var addProbeReq = new AddProbeRequest("Log", "PaymentService", "ProcessPayment", "amount > 100", 30);
        var probeResponse = await client.PostAsJsonAsync($"/api/investigations/{inv!.Id}/probes", addProbeReq);
        Assert.True(probeResponse.IsSuccessStatusCode);
        var probe = await probeResponse.Content.ReadFromJsonAsync<ProbeDto>();
        Assert.NotNull(probe);
        Assert.Equal("Pending", probe!.Status);

        // 3. Activate Probe
        var activateResponse = await client.PostAsync($"/api/probes/{probe.Id}/activate?application=PaymentApi", null);
        Assert.True(activateResponse.IsSuccessStatusCode);
        var activeProbe = await activateResponse.Content.ReadFromJsonAsync<ProbeDto>();
        Assert.NotNull(activeProbe);
        Assert.Equal("Active", activeProbe!.Status);

        // 4. Ingest Evidence / Results
        var evidenceRecords = new List<ProbeEvidenceRecord>
        {
            new(probe.Id, "corr-1", new Dictionary<string, string> { ["customerId"] = "101", ["amount"] = "250.00" }, DateTimeOffset.UtcNow, AgentProtocol.Version, "PaymentService", "ProcessPayment")
        };
        var evidenceResponse = await client.PostAsJsonAsync($"/api/probes/{probe.Id}/evidence", evidenceRecords);
        Assert.True(evidenceResponse.IsSuccessStatusCode);

        // 5. Query Probe Results
        var resultsResponse = await client.GetAsync($"/api/probes/{probe.Id}/results");
        Assert.True(resultsResponse.IsSuccessStatusCode);
        var results = await resultsResponse.Content.ReadFromJsonAsync<IReadOnlyList<ProbeResultDto>>();
        Assert.NotNull(results);
        Assert.Single(results!);
        Assert.Equal("corr-1", results![0].CorrelationId);

        // 6. Deactivate Probe
        var deactResponse = await client.PostAsync($"/api/probes/{probe.Id}/deactivate?reason=InvestigationFinished", null);
        Assert.True(deactResponse.IsSuccessStatusCode);
        var closedProbe = await deactResponse.Content.ReadFromJsonAsync<ProbeDto>();
        Assert.NotNull(closedProbe);
        Assert.Equal("Removed", closedProbe!.Status);
    }

    private sealed record TokenResponse(string AccessToken);
}
