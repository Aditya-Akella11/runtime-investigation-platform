using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RuntimeInvestigation.API;
using Xunit;

namespace API.Tests;

public class DemoWorkflowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public DemoWorkflowTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task DemoWorkflow_ShouldCreateProbeAndCaptureEvidence()
    {
        var client = await CreateAuthenticatedClientAsync();

        var investigationResponse = await client.PostAsJsonAsync("/api/demoWorkflow/investigations", new
        {
            Title = "Payment failures",
            Application = "Payment API",
            Environment = "Production",
            Description = "Investigate duplicate charges"
        });
        await EnsureSuccessOrThrow(investigationResponse);
        var investigation = await investigationResponse.Content.ReadFromJsonAsync<InvestigationResponse>();
        Assert.NotNull(investigation);

        var probeResponse = await client.PostAsJsonAsync("/api/demoWorkflow/probes", new
        {
            InvestigationId = investigation!.Id,
            Application = "Payment API",
            ProbeType = "Log",
            TargetClass = "PaymentService",
            TargetMethod = "ProcessPayment",
            Condition = "Amount > 1000",
            DurationMinutes = 15
        });

        await EnsureSuccessOrThrow(probeResponse);
        var probe = await probeResponse.Content.ReadFromJsonAsync<ProbeResponse>();
        Assert.NotNull(probe);

        var deployResponse = await client.PostAsync($"/api/demoWorkflow/probes/{probe!.Id}/deploy", null);
        await EnsureSuccessOrThrow(deployResponse);

        var evidenceResponse = await client.GetAsync("/api/demoWorkflow/evidence");
        await EnsureSuccessOrThrow(evidenceResponse);
        var evidence = await evidenceResponse.Content.ReadFromJsonAsync<EvidenceResponse[]>();

        Assert.NotNull(evidence);
        Assert.NotEmpty(evidence!);
        Assert.Contains(evidence!, item => item.ProbeId == probe.Id);
    }

    private static async Task EnsureSuccessOrThrow(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        var body = await response.Content.ReadAsStringAsync();
        throw new Exception($"Request failed: {(int)response.StatusCode} {response.ReasonPhrase}\nBody: {body}");
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var tokenResponse = await client.PostAsJsonAsync("/api/auth/token", new { Username = "admin", Password = "password" });
        tokenResponse.EnsureSuccessStatusCode();
        var token = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(token);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token!.AccessToken);
        return client;
    }

    private sealed record TokenResponse(string AccessToken);
    private sealed record InvestigationResponse(string Id, string Title, string Application, string Environment, string Description, string Status, DateTimeOffset CreatedAt);
    private sealed record ProbeResponse(string Id, string InvestigationId, string Application, string ProbeType, string TargetClass, string TargetMethod, string Condition, int DurationMinutes, string Status, DateTimeOffset CreatedAt);
    private sealed record EvidenceResponse(string Id, string ProbeId, string Application, string TargetClass, string TargetMethod, string Payload, DateTimeOffset CapturedAt);
}
