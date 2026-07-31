namespace RuntimeInvestigation.RuntimeAgent.Policy;

public sealed class LocalProbeSafetyPolicy
{
    private static readonly HashSet<string> AllowedCapabilities = new(StringComparer.OrdinalIgnoreCase)
    {
        "log",
        "method-entry",
        "method-exit",
        "parameters",
        "exceptions"
    };

    public bool CanCapture(string capability) => AllowedCapabilities.Contains(capability);

    public bool CanCapturePayload(string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var forbidden = new[] { "password", "secret", "token", "apikey", "privatekey" };
        return forbidden.All(term => !name.Contains(term, StringComparison.OrdinalIgnoreCase))
            && (value is null || value.Length <= 256);
    }
}
