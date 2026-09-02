using System;
using System.Collections.Generic;

namespace RuntimeInvestigation.Domain.Entities;

public class ProbeResult
{
    protected ProbeResult() { }

    public ProbeResult(
        string probeId,
        string correlationId,
        string methodName,
        IReadOnlyDictionary<string, string> arguments,
        string? returnValue,
        double durationMs,
        DateTime capturedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(probeId)) throw new ArgumentException("ProbeId is required.", nameof(probeId));
        if (string.IsNullOrWhiteSpace(methodName)) throw new ArgumentException("MethodName is required.", nameof(methodName));

        Id = Guid.NewGuid().ToString("N");
        ProbeId = probeId.Trim();
        CorrelationId = correlationId ?? Guid.NewGuid().ToString("N");
        MethodName = methodName.Trim();
        Arguments = arguments ?? new Dictionary<string, string>();
        ReturnValue = returnValue;
        DurationMs = durationMs;
        CapturedAtUtc = capturedAtUtc == default ? DateTime.UtcNow : capturedAtUtc.ToUniversalTime();
    }

    public string Id { get; private set; } = string.Empty;
    public string ProbeId { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;
    public string MethodName { get; private set; } = string.Empty;
    public IReadOnlyDictionary<string, string> Arguments { get; private set; } = new Dictionary<string, string>();
    public string? ReturnValue { get; private set; }
    public double DurationMs { get; private set; }
    public DateTime CapturedAtUtc { get; private set; }
}
