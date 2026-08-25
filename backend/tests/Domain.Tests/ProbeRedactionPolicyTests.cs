using RuntimeInvestigation.RuntimeAgent.Safety;
using Xunit;

namespace Domain.Tests;

public class ProbeRedactionPolicyTests
{
    [Theory]
    [InlineData("password", "abc", "[redacted]")]
    [InlineData("secretToken", "secret123", "[redacted]")]
    [InlineData("apiKey", "key-xyz", "[redacted]")]
    [InlineData("Authorization", "Bearer xyz", "[redacted]")]
    [InlineData("ConnectionString", "Server=.", "[redacted]")]
    [InlineData("cvv", "123", "[redacted]")]
    [InlineData("cardNumber", "4111222233334444", "[redacted]")]
    [InlineData("CustomerId", "1452", "1452")]
    [InlineData("Amount", "120.50", "120.50")]
    public void Redact_ShouldHideSensitiveValuesByKeyName(string name, string? value, string expected)
    {
        var policy = new ProbeRedactionPolicy();
        Assert.Equal(expected, policy.Redact(name, value));
    }

    [Fact]
    public void Redact_ShouldMaskCreditCardNumberInGenericValue()
    {
        var policy = new ProbeRedactionPolicy();
        // Visa 16-digit number in generic field
        var result = policy.Redact("payload", "4111111111111111");
        Assert.Equal("[redacted]", result);
    }

    [Fact]
    public void Redact_ShouldMaskJwtTokensInGenericValue()
    {
        var policy = new ProbeRedactionPolicy();
        var jwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dozjgN_xP_wsmwSt_k7b";
        var result = policy.Redact("header", jwt);
        Assert.Equal("[redacted]", result);
    }

    [Fact]
    public void RedactDictionary_ShouldSanitizeAllEntries()
    {
        var policy = new ProbeRedactionPolicy();
        var input = new Dictionary<string, string>
        {
            ["customerId"] = "1001",
            ["password"] = "p@ssword",
            ["amount"] = "500",
            ["token"] = "ey12345"
        };

        var redacted = policy.RedactDictionary(input);
        Assert.Equal("1001", redacted["customerId"]);
        Assert.Equal("[redacted]", redacted["password"]);
        Assert.Equal("500", redacted["amount"]);
        Assert.Equal("[redacted]", redacted["token"]);
    }
}
