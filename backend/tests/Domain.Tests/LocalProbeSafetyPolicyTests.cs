using RuntimeInvestigation.RuntimeAgent.Policy;
using RuntimeInvestigation.Shared.Contracts;
using Xunit;

namespace Domain.Tests;

public class LocalProbeSafetyPolicyTests
{
    [Theory]
    [InlineData("log", true)]
    [InlineData("method-entry", true)]
    [InlineData("method-exit", true)]
    [InlineData("parameters", true)]
    [InlineData("exceptions", true)]
    [InlineData("arbitrary-exec", false)]
    [InlineData("unknown", false)]
    public void CanCapture_ShouldAllowKnownCapabilities(string capability, bool expected)
    {
        var policy = new LocalProbeSafetyPolicy();
        Assert.Equal(expected, policy.CanCapture(capability));
    }

    [Theory]
    [InlineData("password", "value", false)]
    [InlineData("secretKey", "secret-token", false)]
    [InlineData("token", "bearer abc", false)]
    [InlineData("apiKey", "key-12345", false)]
    [InlineData("CustomerId", "123", true)]
    [InlineData("Amount", "99.95", true)]
    public void CanCapturePayload_ShouldBlockSecrets(string name, string value, bool expected)
    {
        var policy = new LocalProbeSafetyPolicy();
        Assert.Equal(expected, policy.CanCapturePayload(name, value));
    }

    [Fact]
    public void ValidateActivation_ValidCommand_ShouldSucceed()
    {
        var policy = new LocalProbeSafetyPolicy();
        var cmd = new ProbeActivationCommand(
            "probe-1",
            "Payment API",
            "PaymentService",
            "ProcessPayment",
            DateTimeOffset.UtcNow.AddMinutes(30),
            AgentProtocol.Version);

        var (isAllowed, reason) = policy.ValidateActivation(cmd);
        Assert.True(isAllowed);
        Assert.Null(reason);
    }

    [Fact]
    public void ValidateActivation_ProtectedNamespace_ShouldReject()
    {
        var policy = new LocalProbeSafetyPolicy();
        var cmd = new ProbeActivationCommand(
            "probe-1",
            "Payment API",
            "System.Security.Cryptography.Aes",
            "Decrypt",
            DateTimeOffset.UtcNow.AddMinutes(30),
            AgentProtocol.Version);

        var (isAllowed, reason) = policy.ValidateActivation(cmd);
        Assert.False(isAllowed);
        Assert.Contains("protected security namespace", reason);
    }

    [Fact]
    public void ValidateActivation_ExpiredOrExceedingDuration_ShouldReject()
    {
        var policy = new LocalProbeSafetyPolicy();

        // Expired
        var expiredCmd = new ProbeActivationCommand(
            "probe-1",
            "Payment API",
            "PaymentService",
            "ProcessPayment",
            DateTimeOffset.UtcNow.AddMinutes(-5),
            AgentProtocol.Version);

        var (isAllowedPast, reasonPast) = policy.ValidateActivation(expiredCmd);
        Assert.False(isAllowedPast);
        Assert.Contains("future", reasonPast);

        // Exceeding max duration (e.g. 300 min)
        var excessiveCmd = new ProbeActivationCommand(
            "probe-1",
            "Payment API",
            "PaymentService",
            "ProcessPayment",
            DateTimeOffset.UtcNow.AddMinutes(300),
            AgentProtocol.Version);

        var (isAllowedExcess, reasonExcess) = policy.ValidateActivation(excessiveCmd);
        Assert.False(isAllowedExcess);
        Assert.Contains("maximum allowed limit", reasonExcess);
    }
}
