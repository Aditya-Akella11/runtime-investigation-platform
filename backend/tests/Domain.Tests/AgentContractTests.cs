using RuntimeInvestigation.Shared.Contracts;

namespace Domain.Tests;

public class AgentContractTests
{
    [Fact]
    public void ProtocolVersion_ShouldBeV1()
    {
        Assert.Equal("v1", AgentProtocol.Version);
    }

    [Fact]
    public void Contracts_ShouldRoundTripData()
    {
        var registration = new AgentRegistrationCommand("agent-1", ["log", "exception"], AgentProtocol.Version);
        var activation = new ProbeActivationCommand("probe-1", "Payment API", "PaymentService", "ProcessPayment", DateTimeOffset.UtcNow, AgentProtocol.Version);
        var evidence = new ProbeEvidenceRecord("probe-1", "corr-1", new Dictionary<string, string> { ["amount"] = "120" }, DateTimeOffset.UtcNow, AgentProtocol.Version);
        var removal = new ProbeRemovalCommand("probe-1", "expired", AgentProtocol.Version);

        Assert.Equal("agent-1", registration.AgentId);
        Assert.Equal("PaymentService", activation.TargetClass);
        Assert.Equal("120", evidence.Values["amount"]);
        Assert.Equal("expired", removal.Reason);
    }
}
