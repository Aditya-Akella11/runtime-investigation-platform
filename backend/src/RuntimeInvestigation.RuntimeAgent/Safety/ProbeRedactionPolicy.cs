using System.Text.RegularExpressions;

namespace RuntimeInvestigation.RuntimeAgent.Safety;

public sealed partial class ProbeRedactionPolicy
{
    private static readonly string[] SensitiveKeyTerms =
    [
        "password",
        "secret",
        "token",
        "apikey",
        "privatekey",
        "connectionstring",
        "authorization",
        "cvv",
        "cvc",
        "creditcard",
        "cardnumber",
        "ssn"
    ];

    private static readonly Regex CreditCardPattern = new(
        @"\b(?:4[0-9]{12}(?:[0-9]{3})?|5[1-5][0-9]{14}|3[47][0-9]{13}|6(?:011|5[0-9]{2})[0-9]{12})\b",
        RegexOptions.Compiled);

    private static readonly Regex JwtPattern = new(
        @"eyJ[a-zA-Z0-9_-]{10,}\.eyJ[a-zA-Z0-9_-]{10,}\.[a-zA-Z0-9_-]{10,}",
        RegexOptions.Compiled);

    public const string RedactedMask = "[redacted]";

    public string Redact(string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return RedactedMask;
        }

        if (SensitiveKeyTerms.Any(term => name.Contains(term, StringComparison.OrdinalIgnoreCase)))
        {
            return RedactedMask;
        }

        if (value is null)
        {
            return "[null]";
        }

        if (CreditCardPattern.IsMatch(value) || JwtPattern.IsMatch(value))
        {
            return RedactedMask;
        }

        return value.Length > 64 ? value[..64] + "..." : value;
    }

    public IReadOnlyDictionary<string, string> RedactDictionary(IReadOnlyDictionary<string, string> values)
    {
        var redacted = new Dictionary<string, string>();
        foreach (var (k, v) in values)
        {
            redacted[k] = Redact(k, v);
        }
        return redacted;
    }
}
