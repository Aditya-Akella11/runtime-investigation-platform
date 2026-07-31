using RuntimeInvestigation.RuntimeAgent.Safety;

namespace Domain.Tests;

public class ProbeRedactionPolicyTests
{
    [Theory]
    [InlineData("password", "abc", "[redacted]")]
    [InlineData("ConnectionString", "Server=.", "[redacted]")]
    [InlineData("CustomerId", "1452", "1452")]
    public void Redact_ShouldHideSensitiveValues(string name, string? value, string expected)
    {
        var policy = new ProbeRedactionPolicy();

        Assert.Equal(expected, policy.Redact(name, value));
    }
}
