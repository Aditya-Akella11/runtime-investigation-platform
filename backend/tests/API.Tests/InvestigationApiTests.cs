using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RuntimeInvestigation.API;
using RuntimeInvestigation.Application.Features.Investigations;
using RuntimeInvestigation.Infrastructure.Persistence.Repositories;
using Xunit;

namespace API.Tests;

public class InvestigationApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public InvestigationApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace Mongo repo with in-memory repo for integration test
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IInvestigationRepository));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddSingleton<IInvestigationRepository, InMemoryInvestigationRepository>();
            });
        });
    }

    [Fact]
    public async Task Investigations_ShouldRequireAuthentication()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/investigations");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateAndGetInvestigation_ShouldSucceed()
    {
        var client = await CreateAuthenticatedClientAsync();

        var createResponse = await client.PostAsJsonAsync("/api/investigations", new
        {
            ApplicationId = "app-checkout",
            Title = "Checkout latency spike",
            Description = "Investigate 3s latency in checkout"
        });

        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<InvestigationDto>();
        Assert.NotNull(created);
        Assert.Equal("Checkout latency spike", created!.Title);
        Assert.Equal("Open", created.Status);

        // Get by ID
        var getResponse = await client.GetAsync($"/api/investigations/{created.Id}");
        getResponse.EnsureSuccessStatusCode();
        var fetched = await getResponse.Content.ReadFromJsonAsync<InvestigationDto>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);

        // Transition: Start
        var startResponse = await client.PostAsync($"/api/investigations/{created.Id}/start", null);
        startResponse.EnsureSuccessStatusCode();
        var started = await startResponse.Content.ReadFromJsonAsync<InvestigationDto>();
        Assert.Equal("Investigating", started!.Status);

        // Transition: Resolve
        var resolveResponse = await client.PostAsync($"/api/investigations/{created.Id}/resolve", null);
        resolveResponse.EnsureSuccessStatusCode();
        var resolved = await resolveResponse.Content.ReadFromJsonAsync<InvestigationDto>();
        Assert.Equal("Resolved", resolved!.Status);
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
