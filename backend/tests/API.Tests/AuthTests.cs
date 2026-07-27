using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RuntimeInvestigation.API;
using Xunit;

namespace API.Tests;

public class AuthTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task TokenEndpoint_ShouldReturnJwtToken()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/token", new { Username = "admin", Password = "password" });

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        Assert.Contains("accessToken", json);
    }

    [Fact]
    public async Task ApplicationsEndpoint_ShouldRequireAuthentication()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/applications");

        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
