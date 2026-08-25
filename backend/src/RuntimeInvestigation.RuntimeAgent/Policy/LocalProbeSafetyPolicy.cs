using RuntimeInvestigation.Shared.Contracts;

namespace RuntimeInvestigation.RuntimeAgent.Policy;

public sealed class LocalProbeSafetyPolicy
{
    private static readonly HashSet<string> AllowedCapabilities = new(StringComparer.OrdinalIgnoreCase)
    {
        AgentProtocol.Capabilities.Log,
        AgentProtocol.Capabilities.MethodEntry,
        AgentProtocol.Capabilities.MethodExit,
        AgentProtocol.Capabilities.Parameters,
        AgentProtocol.Capabilities.Exceptions
    };

    private static readonly string[] ForbiddenNamespaces =
    [
        "System.Security",
        "System.Runtime.InteropServices",
        "Microsoft.AspNetCore.Authentication",
        "Microsoft.AspNetCore.Identity",
        "Microsoft.IdentityModel"
    ];

    private static readonly string[] ForbiddenKeywords =
    [
        "password",
        "secret",
        "token",
        "apikey",
        "privatekey",
        "credential",
        "connectionstring"
    ];

    public const int MaxAllowedDurationMinutes = 120;
    public const int MaxAllowedRateLimit = 1000;
    public const int MaxPayloadStringLength = 512;

    public bool CanCapture(string capability) =>
        !string.IsNullOrWhiteSpace(capability) && AllowedCapabilities.Contains(capability);

    public bool CanCapturePayload(string name, string? value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        if (ForbiddenKeywords.Any(term => name.Contains(term, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return value is null || value.Length <= MaxPayloadStringLength;
    }

    public (bool IsAllowed, string? Reason) ValidateActivation(ProbeActivationCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.ProbeId))
        {
            return (false, "ProbeId is required.");
        }

        if (string.IsNullOrWhiteSpace(command.TargetClass) || string.IsNullOrWhiteSpace(command.TargetMethod))
        {
            return (false, "TargetClass and TargetMethod are required.");
        }

        if (ForbiddenNamespaces.Any(ns => command.TargetClass.StartsWith(ns, StringComparison.OrdinalIgnoreCase)))
        {
            return (false, $"Target class '{command.TargetClass}' is in a protected security namespace.");
        }

        if (command.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            return (false, "Probe expiration must be in the future.");
        }

        if (command.ExpiresAtUtc > DateTimeOffset.UtcNow.AddMinutes(MaxAllowedDurationMinutes))
        {
            return (false, $"Probe duration exceeds maximum allowed limit of {MaxAllowedDurationMinutes} minutes.");
        }

        if (command.RateLimitPerSecond <= 0 || command.RateLimitPerSecond > MaxAllowedRateLimit)
        {
            return (false, $"Rate limit must be between 1 and {MaxAllowedRateLimit} events/second.");
        }

        if (command.ProtocolVersion != AgentProtocol.Version)
        {
            return (false, $"Unsupported protocol version '{command.ProtocolVersion}'. Expected '{AgentProtocol.Version}'.");
        }

        return (true, null);
    }
}
