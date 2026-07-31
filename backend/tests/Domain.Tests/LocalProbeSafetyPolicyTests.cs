using RuntimeInvestigation.RuntimeAgent.Policy;

namespace Domain.Tests;

public class LocalProbeSafetyPolicyTests
{
    [Theory]
    [InlineData("log", true)]
    [InlineData("method-entry", true)]
    [InlineData("unknown", false)]
    public void CanCapture_ShouldAllowKnownCapabilities(string capability, bool expected)
    {
        var policy = new LocalProbeSafetyPolicy();

        Assert.Equal(expected, policy.CanCapture(capability));
    }

    [Theory]
    [InlineData("password", "value", false)]
    [InlineData("CustomerId", "123", true)]
    public void CanCapturePayload_ShouldBlockSecrets(string name, string value, bool expected)
    {
        var policy = new LocalProbeSafetyPolicy();

        Assert.Equal(expected, policy.CanCapturePayload(name, value));
    }
}
