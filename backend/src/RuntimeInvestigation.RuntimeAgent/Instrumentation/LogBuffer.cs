using System;
using System.Collections.Concurrent;
using System.Threading;
using RuntimeInvestigation.RuntimeAgent.Safety;

namespace RuntimeInvestigation.RuntimeAgent.Instrumentation;

public sealed record LogBufferEntry(
    string ProbeId,
    string MethodName,
    IReadOnlyDictionary<string, string> Arguments,
    string? ReturnValue,
    DateTimeOffset TimestampUtc);

public sealed class LogBuffer
{
    private readonly ConcurrentQueue<LogBufferEntry> _queue = new();
    private readonly ProbeRedactionPolicy _redactionPolicy;
    private int _droppedCount;

    public int MaxCapacity { get; }
    public int DroppedCount => _droppedCount;
    public int Count => _queue.Count;

    public LogBuffer(int maxCapacity, ProbeRedactionPolicy redactionPolicy)
    {
        if (maxCapacity <= 0) throw new ArgumentOutOfRangeException(nameof(maxCapacity), "Max capacity must be positive.");
        MaxCapacity = maxCapacity;
        _redactionPolicy = redactionPolicy;
    }

    /// <summary>
    /// Non-blocking append. If at capacity, drops the entry and increments DroppedCount.
    /// </summary>
    public bool TryAppend(string probeId, string methodName, IReadOnlyDictionary<string, string> rawArguments, string? returnValue = null)
    {
        if (_queue.Count >= MaxCapacity)
        {
            Interlocked.Increment(ref _droppedCount);
            return false;
        }

        var redacted = _redactionPolicy.RedactDictionary(rawArguments);
        string? redactedReturn = returnValue is null ? null : _redactionPolicy.Redact("returnValue", returnValue);

        _queue.Enqueue(new LogBufferEntry(probeId, methodName, redacted, redactedReturn, DateTimeOffset.UtcNow));
        return true;
    }

    /// <summary>
    /// Drains all entries from the buffer, returning them in capture order.
    /// </summary>
    public IReadOnlyList<LogBufferEntry> Drain()
    {
        var entries = new List<LogBufferEntry>(_queue.Count);
        while (_queue.TryDequeue(out var entry))
        {
            entries.Add(entry);
        }
        return entries;
    }

    /// <summary>
    /// Returns a snapshot of current entries without removing them.
    /// </summary>
    public IReadOnlyList<LogBufferEntry> Peek() => _queue.ToArray();

    public void Clear()
    {
        while (_queue.TryDequeue(out _)) { }
    }
}
