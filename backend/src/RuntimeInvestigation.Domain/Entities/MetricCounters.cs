namespace RuntimeInvestigation.Domain.Entities;

/// <summary>Mutable metric counters attached to a MetricProbe instance.</summary>
public sealed class MetricCounters
{
    public string ProbeId { get; init; } = string.Empty;
    public long   CallCount      { get; set; }
    public long   TotalDurationMs { get; set; }
    public long   ExceptionCount { get; set; }
    public DateTime LastUpdated  { get; set; } = DateTime.UtcNow;

    public MetricCounters() { }

    public MetricCounters(string probeId, long callCount, long totalDurationMs, long exceptionCount, DateTime lastUpdated)
    {
        ProbeId = probeId;
        CallCount = callCount;
        TotalDurationMs = totalDurationMs;
        ExceptionCount = exceptionCount;
        LastUpdated = lastUpdated;
    }

    public void RecordCall(long durationMs, bool hadException = false)
    {
        CallCount++;
        TotalDurationMs += durationMs;
        if (hadException) ExceptionCount++;
        LastUpdated = DateTime.UtcNow;
    }

    public double AverageDurationMs =>
        CallCount == 0 ? 0 : (double)TotalDurationMs / CallCount;
}
