using RuntimeInvestigation.Domain.Entities;

namespace RuntimeInvestigation.RuntimeAgent.Instrumentation;

/// <summary>
/// Captures method argument snapshots at runtime, applying an optional condition filter.
/// </summary>
public sealed class SnapshotCollector
{
    private readonly List<SnapshotCapture> _captures = new();
    private readonly object _lock = new();

    public string ProbeId { get; }
    public string? Condition { get; }

    public SnapshotCollector(string probeId, string? condition = null)
    {
        ProbeId   = probeId;
        Condition = condition;
    }

    /// <summary>
    /// Called at each method entry. Evaluates the condition (if any) and stores a snapshot.
    /// </summary>
    public void OnEntry(object?[] args)
    {
        if (!ConditionEvaluator.Evaluate(Condition, args)) return;

        var capture = SnapshotCapture.FromArgs(ProbeId, args);
        lock (_lock) { _captures.Add(capture); }
    }

    public IReadOnlyList<SnapshotCapture> GetCaptures()
    {
        lock (_lock) { return _captures.ToList(); }
    }

    public void Clear()
    {
        lock (_lock) { _captures.Clear(); }
    }
}
