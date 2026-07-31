namespace RuntimeInvestigation.RuntimeAgent.Safety;

public sealed class ProbeRedactionPolicy
{
    private static readonly string[] SensitiveTerms =
    [
        "password",
        "secret",
        "token",
        "apikey",
        "privatekey",
        "connectionstring"
    ];

    public string Redact(string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "[redacted]";
        }

        if (SensitiveTerms.Any(term => name.Contains(term, StringComparison.OrdinalIgnoreCase)))
        {
            return "[redacted]";
        }

        if (value is null)
        {
            return "[null]";
        }

        return value.Length > 64 ? value[..64] + "..." : value;
    }
}
