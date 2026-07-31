namespace RuntimeInvestigation.RuntimeAgent.Diagnostics;

public sealed class AgentCapabilityCatalog
{
    private static readonly string[] Capabilities =
    [
        "log",
        "method-entry",
        "method-exit",
        "parameters",
        "exceptions"
    ];

    public IReadOnlyList<string> GetCapabilities() => Capabilities;
}

public sealed class RuntimeAgentDiagnostics
{
    private readonly DateTimeOffset _startedAt = DateTimeOffset.UtcNow;

    public object Snapshot() => new
    {
        startedAt = _startedAt,
        version = "phase2-skeleton",
        runtime = Environment.Version.ToString()
    };
}
