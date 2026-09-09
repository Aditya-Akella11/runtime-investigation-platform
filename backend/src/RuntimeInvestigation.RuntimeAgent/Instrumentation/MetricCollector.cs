using System.Diagnostics;
using RuntimeInvestigation.Domain.Entities;

namespace RuntimeInvestigation.RuntimeAgent.Instrumentation;

/// <summary>
/// Thread-safe metric collector for a single probe.
/// Uses Interlocked for lock-free counter increments.
/// </summary>
public sealed class MetricCollector
{
    private long _callCount;
    private long _totalDurationMs;
    private long _exceptionCount;

    public string ProbeId { get; }

    public MetricCollector(string probeId) => ProbeId = probeId;

    public IDisposable MeasureCall() => new CallScope(this);

    public void RecordException() => Interlocked.Increment(ref _exceptionCount);

    public MetricCounters Snapshot()
    {
        var (callCount, totalDurationMs, exceptionCount) = GetSnapshot();
        return new MetricCounters(ProbeId, callCount, totalDurationMs, exceptionCount, DateTime.UtcNow);
    }

    public (long callCount, long totalDurationMs, long exceptionCount) GetSnapshot() =>
        (Interlocked.Read(ref _callCount),
         Interlocked.Read(ref _totalDurationMs),
         Interlocked.Read(ref _exceptionCount));

    private void Complete(long durationMs, bool hadException)
    {
        Interlocked.Increment(ref _callCount);
        Interlocked.Add(ref _totalDurationMs, durationMs);
        if (hadException) Interlocked.Increment(ref _exceptionCount);
    }

    private sealed class CallScope : IDisposable
    {
        private readonly MetricCollector _owner;
        private readonly Stopwatch _sw = Stopwatch.StartNew();
        private bool _hadException;

        public CallScope(MetricCollector owner) => _owner = owner;

        public void MarkException() => _hadException = true;

        public void Dispose()
        {
            _sw.Stop();
            _owner.Complete(_sw.ElapsedMilliseconds, _hadException);
        }
    }
}
