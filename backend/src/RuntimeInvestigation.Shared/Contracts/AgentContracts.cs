namespace RuntimeInvestigation.Shared.Contracts;

public static class AgentProtocol
{
    public const string Version = "v1";

    public static class Capabilities
    {
        public const string Log = "log";
        public const string MethodEntry = "method-entry";
        public const string MethodExit = "method-exit";
        public const string Parameters = "parameters";
        public const string Exceptions = "exceptions";

        public static readonly IReadOnlyList<string> All =
        [
            Log,
            MethodEntry,
            MethodExit,
            Parameters,
            Exceptions
        ];
    }
}

public sealed record AgentRegistrationCommand(
    string AgentId,
    IReadOnlyList<string> Capabilities,
    string ProtocolVersion);

public sealed record AgentRegistrationAck(
    string AgentId,
    string ProtocolVersion,
    IReadOnlyList<string> ApprovedCapabilities,
    bool Success,
    string? ErrorMessage = null);

public sealed record ProbeActivationCommand(
    string ProbeId,
    string Application,
    string TargetClass,
    string TargetMethod,
    DateTimeOffset ExpiresAtUtc,
    string ProtocolVersion,
    string? Condition = null,
    int RateLimitPerSecond = 100);

public sealed record ProbeActivationAck(
    string ProbeId,
    string Status,
    string ProtocolVersion,
    bool Success,
    string? ErrorMessage = null);

public sealed record ProbeEvidenceRecord(
    string ProbeId,
    string CorrelationId,
    IReadOnlyDictionary<string, string> Values,
    DateTimeOffset CapturedAtUtc,
    string ProtocolVersion,
    string? TargetClass = null,
    string? TargetMethod = null);

public sealed record ProbeRemovalCommand(
    string ProbeId,
    string Reason,
    string ProtocolVersion);

public sealed record ProbeRemovalAck(
    string ProbeId,
    string Status,
    string ProtocolVersion,
    bool Success);

public sealed record AgentHeartbeatMessage(
    string AgentId,
    string ProtocolVersion,
    DateTimeOffset TimestampUtc,
    int ActiveProbeCount);
