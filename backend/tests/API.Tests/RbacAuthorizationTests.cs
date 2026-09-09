using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RuntimeInvestigation.API;
using RuntimeInvestigation.API.Controllers;
using RuntimeInvestigation.Application.Features.Investigations;
using RuntimeInvestigation.Application.Features.Probes;
using RuntimeInvestigation.Application.Features.Users;
using RuntimeInvestigation.Infrastructure.Persistence.Repositories;
using Xunit;

namespace API.Tests;

public class RbacAuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public RbacAuthorizationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped<IInvestigationRepository, InMemoryInvestigationRepository>();
                services.AddScoped<IProbeRepository, InMemoryProbeRepository>();
                services.AddScoped<IUserRepository, InMemoryUserRepository>();
            });
        });
    }

    private async Task<HttpClient> CreateClientWithRoleAsync(string role, string tenantId = "default")
    {
        var client = _factory.CreateClient();
        var tokenResponse = await client.PostAsJsonAsync("/api/auth/token", new
        {
            Username = role.ToLowerInvariant(),
            Password = "password",
            TenantId = tenantId,
            Role = role
        });
        tokenResponse.EnsureSuccessStatusCode();
        var token = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);
        return client;
    }

    [Fact]
    public async Task CreateProbe_WithNoToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/investigations/inv-1/probes", new AddProbeRequest("Log", "TestClass", "TestMethod"));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateProbe_WithViewerRole_ReturnsForbidden()
    {
        var client = await CreateClientWithRoleAsync("Viewer");
        var response = await client.PostAsJsonAsync("/api/investigations/inv-1/probes", new AddProbeRequest("Log", "TestClass", "TestMethod"));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateProbe_WithAdminRole_ReturnsCreated()
    {
        var client = await CreateClientWithRoleAsync("Admin");
        var response = await client.PostAsJsonAsync("/api/investigations/inv-1/probes", new AddProbeRequest("Log", "TestClass", "TestMethod"));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithAdminRole_ReturnsCreated()
    {
        var client = await CreateClientWithRoleAsync("Admin");
        var response = await client.PostAsJsonAsync("/api/users", new CreateUserRequest("newuser@example.com", "New User", "Investigator"));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithViewerRole_ReturnsForbidden()
    {
        var client = await CreateClientWithRoleAsync("Viewer");
        var response = await client.PostAsJsonAsync("/api/users", new CreateUserRequest("baduser@example.com", "Bad User", "Viewer"));
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WithViewerRole_ReturnsForbidden()
    {
        var client = await CreateClientWithRoleAsync("Viewer");
        var response = await client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
