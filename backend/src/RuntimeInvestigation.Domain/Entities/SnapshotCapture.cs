namespace RuntimeInvestigation.Domain.Entities;

/// <summary>Value object representing a captured variable snapshot at a probe site.</summary>
public sealed record SnapshotCapture(
    string ProbeId,
    DateTime CapturedAt,
    IReadOnlyDictionary<string, string?> Variables)
{
    public static SnapshotCapture FromArgs(string probeId, object?[] args)
    {
        var vars = new Dictionary<string, string?>(args.Length);
        for (int i = 0; i < args.Length; i++)
            vars[$"args[{i}]"] = args[i]?.ToString();
        return new SnapshotCapture(probeId, DateTime.UtcNow, vars);
    }
}
