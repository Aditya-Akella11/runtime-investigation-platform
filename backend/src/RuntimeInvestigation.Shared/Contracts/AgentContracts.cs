namespace RuntimeInvestigation.Shared.Contracts;

public static class AgentProtocol
{
    public const string Version = "v1";
}

public sealed record AgentRegistrationCommand(string AgentId, IReadOnlyList<string> Capabilities, string ProtocolVersion);
public sealed record ProbeActivationCommand(string ProbeId, string Application, string TargetClass, string TargetMethod, DateTimeOffset ExpiresAtUtc, string ProtocolVersion);
public sealed record ProbeEvidenceRecord(string ProbeId, string CorrelationId, IReadOnlyDictionary<string, string> Values, DateTimeOffset CapturedAtUtc, string ProtocolVersion);
public sealed record ProbeRemovalCommand(string ProbeId, string Reason, string ProtocolVersion);
