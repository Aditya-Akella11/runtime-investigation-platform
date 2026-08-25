using System.Diagnostics;
using RuntimeInvestigation.Shared.Contracts;

namespace RuntimeInvestigation.RuntimeAgent.Diagnostics;

public sealed class AgentCapabilityCatalog
{
    public IReadOnlyList<string> GetCapabilities() => AgentProtocol.Capabilities.All;
}

public sealed class RuntimeAgentDiagnostics
{
    private readonly DateTimeOffset _startedAt = DateTimeOffset.UtcNow;
    private int _activeProbeCount;
    private long _totalCapturedEvents;

    public void SetActiveProbeCount(int count) => Interlocked.Exchange(ref _activeProbeCount, count);
    public void IncrementCapturedEvents() => Interlocked.Increment(ref _totalCapturedEvents);

    public object Snapshot()
    {
        var process = Process.GetCurrentProcess();
        return new
        {
            status = "healthy",
            version = "phase2-production-ready",
            protocol = AgentProtocol.Version,
            startedAt = _startedAt,
            uptimeSeconds = (DateTimeOffset.UtcNow - _startedAt).TotalSeconds,
            activeProbes = _activeProbeCount,
            totalCapturedEvents = Interlocked.Read(ref _totalCapturedEvents),
            process = new
            {
                workingSetMB = process.WorkingSet64 / (1024 * 1024),
                threadCount = process.Threads.Count,
                runtimeVersion = Environment.Version.ToString()
            }
        };
    }
}
