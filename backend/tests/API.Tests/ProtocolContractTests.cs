using System.Text.Json;
using RuntimeInvestigation.Shared.Contracts;
using Xunit;

namespace API.Tests;

public class ProtocolContractTests
{
    [Fact]
    public void AgentRegistrationCommand_ShouldSerializeAndDeserializeJson()
    {
        var original = new AgentRegistrationCommand("agent-007", AgentProtocol.Capabilities.All, AgentProtocol.Version);
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<AgentRegistrationCommand>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.AgentId, deserialized!.AgentId);
        Assert.Equal(original.ProtocolVersion, deserialized.ProtocolVersion);
        Assert.Equal(original.Capabilities.Count, deserialized.Capabilities.Count);
    }

    [Fact]
    public void ProbeActivationCommand_ShouldSerializeAndDeserializeJson()
    {
        var original = new ProbeActivationCommand(
            "probe-42",
            "Payment API",
            "PaymentService",
            "ProcessPayment",
            DateTimeOffset.UtcNow.AddMinutes(15),
            AgentProtocol.Version,
            "Amount > 500",
            50);

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<ProbeActivationCommand>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.ProbeId, deserialized!.ProbeId);
        Assert.Equal(original.Condition, deserialized.Condition);
        Assert.Equal(original.RateLimitPerSecond, deserialized.RateLimitPerSecond);
    }

    [Fact]
    public void ProbeEvidenceRecord_ShouldSerializeAndDeserializeJson()
    {
        var values = new Dictionary<string, string>
        {
            ["customerId"] = "cust-1",
            ["amount"] = "99.99"
        };

        var original = new ProbeEvidenceRecord(
            "probe-42",
            "corr-123",
            values,
            DateTimeOffset.UtcNow,
            AgentProtocol.Version,
            "PaymentService",
            "ProcessPayment");

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<ProbeEvidenceRecord>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.ProbeId, deserialized!.ProbeId);
        Assert.Equal(original.CorrelationId, deserialized.CorrelationId);
        Assert.Equal("99.99", deserialized.Values["amount"]);
    }
}
