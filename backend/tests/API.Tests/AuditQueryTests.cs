using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RuntimeInvestigation.API;
using RuntimeInvestigation.API.Services;
using Xunit;

namespace API.Tests;

public class AuditQueryTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuditQueryTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AuditTrail_ShouldBeQueryableAndFilterable()
    {
        var client = await CreateAuthenticatedClientAsync();

        // Query all audit logs
        var auditResponse = await client.GetAsync("/api/demoWorkflow/audit");
        auditResponse.EnsureSuccessStatusCode();
        var allAudit = await auditResponse.Content.ReadFromJsonAsync<AuditEntry[]>();
        Assert.NotNull(allAudit);
        Assert.NotEmpty(allAudit!);

        // Create an investigation and verify audit entry exists
        var invResponse = await client.PostAsJsonAsync("/api/demoWorkflow/investigations", new
        {
            Title = "Audit Test Investigation",
            Application = "Payment API",
            Environment = "Production",
            Description = "Testing audit recording"
        });
        invResponse.EnsureSuccessStatusCode();
        var created = await invResponse.Content.ReadFromJsonAsync<InvestigationRecord>();
        Assert.NotNull(created);

        // Filter audit by subject ID
        var filteredResponse = await client.GetAsync($"/api/demoWorkflow/audit?subjectId={created!.Id}");
        filteredResponse.EnsureSuccessStatusCode();
        var filteredAudit = await filteredResponse.Content.ReadFromJsonAsync<AuditEntry[]>();
        Assert.NotNull(filteredAudit);
        Assert.Contains(filteredAudit!, entry => entry.SubjectId == created.Id);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var tokenResponse = await client.PostAsJsonAsync("/api/auth/token", new { Username = "admin", Password = "password" });
        tokenResponse.EnsureSuccessStatusCode();
        var token = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
        Assert.NotNull(token);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);
        return client;
    }

    private sealed record TokenResponse(string AccessToken);
}
