using Microsoft.AspNetCore.Mvc.Testing;
using RuntimeInvestigation.API;
using RuntimeInvestigation.API.Services;
using Xunit;

namespace API.Tests;

public class HealthAndDispatcherTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthAndDispatcherTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ReadyHealthEndpoint_ShouldReturnOk()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/ready");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task MockDispatcher_ShouldRecordAuditWhenDeploying()
    {
        var store = new DemoWorkflowStore();
        var dispatcher = new MockAgentProbeDispatcher(store);

        await dispatcher.DeployAsync("probe-1", "Payment API", "PaymentService", "ProcessPayment", DateTimeOffset.UtcNow.AddMinutes(15));

        Assert.Contains(store.GetAudit(), entry => entry.EventType == "ProbeDispatched" && entry.SubjectId == "probe-1");
    }
}
