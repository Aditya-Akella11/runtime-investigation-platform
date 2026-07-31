using RuntimeInvestigation.RuntimeAgent.Diagnostics;

namespace Domain.Tests;

public class RuntimeAgentTests
{
    [Fact]
    public void CapabilityCatalog_ShouldExposeSupportedCapabilities()
    {
        var catalog = new AgentCapabilityCatalog();
        var capabilities = catalog.GetCapabilities();

        Assert.Contains("log", capabilities);
        Assert.Contains("exceptions", capabilities);
    }
}
