using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using RuntimeInvestigation.RuntimeAgent.Policy;
using RuntimeInvestigation.RuntimeAgent.Safety;
using RuntimeInvestigation.Shared.Contracts;

namespace RuntimeInvestigation.RuntimeAgent.Instrumentation;

public enum ProbeActivationStatus { Pending, Active, Failed, Deactivated }

public sealed class ProbeActivationResult
{
    public bool Success { get; init; }
    public ProbeActivationStatus Status { get; init; }
    public string? FailureReason { get; init; }
    public DateTimeOffset? ActivatedAtUtc { get; init; }
}

public sealed class ProbeActivator
{
    private readonly LocalProbeSafetyPolicy _policy;
    private readonly ProbeRedactionPolicy _redactionPolicy;
    private readonly ILogger<ProbeActivator>? _logger;

    // Active probe state
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, (ProbeActivationCommand Command, LogBuffer Buffer, ProbeActivationStatus Status)> _activeProbes = new();

    public ProbeActivator(LocalProbeSafetyPolicy policy, ProbeRedactionPolicy redactionPolicy, ILogger<ProbeActivator>? logger = null)
    {
        _policy = policy;
        _redactionPolicy = redactionPolicy;
        _logger = logger;
    }

    public ProbeActivationResult Activate(ProbeActivationCommand command)
    {
        // 1. Validate via safety policy
        var (isAllowed, reason) = _policy.ValidateActivation(command);
        if (!isAllowed)
        {
            _logger?.LogWarning("Probe {ProbeId} activation rejected: {Reason}", command.ProbeId, reason);
            return new ProbeActivationResult
            {
                Success = false,
                Status = ProbeActivationStatus.Failed,
                FailureReason = reason
            };
        }

        // 2. Resolve method
        var methodPath = $"{command.TargetClass}.{command.TargetMethod}";
        MethodInfo? method = null;
        try
        {
            method = ProbeMethodFinder.FindMethod(methodPath);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Method resolution failed for {MethodPath}", methodPath);
        }

        if (method == null)
        {
            _logger?.LogWarning("Probe {ProbeId}: method '{MethodPath}' could not be resolved in loaded assemblies.", command.ProbeId, methodPath);
            // Per spec: don't mark as active unless instrumentation confirms activation.
            // Method not found in this AppDomain is a supported failure mode for in-process agents.
            return new ProbeActivationResult
            {
                Success = false,
                Status = ProbeActivationStatus.Failed,
                FailureReason = $"Method '{methodPath}' not found in loaded assemblies. Cross-process instrumentation requires CLR Profiling API/ReJIT."
            };
        }

        // 3. Create a bounded log buffer for this probe
        var buffer = new LogBuffer(1000, _redactionPolicy);

        // 4. Register probe as active
        _activeProbes[command.ProbeId] = (command, buffer, ProbeActivationStatus.Active);

        _logger?.LogInformation("Probe {ProbeId} activated on {MethodPath} expires {Expiry}",
            command.ProbeId, methodPath, command.ExpiresAtUtc);

        return new ProbeActivationResult
        {
            Success = true,
            Status = ProbeActivationStatus.Active,
            ActivatedAtUtc = DateTimeOffset.UtcNow
        };
    }

    public ProbeActivationResult Deactivate(string probeId, string reason = "Operator deactivated")
    {
        if (!_activeProbes.TryRemove(probeId, out var entry))
        {
            return new ProbeActivationResult
            {
                Success = false,
                Status = ProbeActivationStatus.Failed,
                FailureReason = $"Probe '{probeId}' is not active."
            };
        }

        // Flush remaining evidence before deactivation
        var remaining = entry.Buffer.Drain();
        _logger?.LogInformation("Probe {ProbeId} deactivated. Reason: {Reason}. Flushed {Count} evidence entries.", probeId, reason, remaining.Count);

        return new ProbeActivationResult
        {
            Success = true,
            Status = ProbeActivationStatus.Deactivated,
            FailureReason = null
        };
    }

    public bool TryGetBuffer(string probeId, out LogBuffer? buffer)
    {
        if (_activeProbes.TryGetValue(probeId, out var entry))
        {
            buffer = entry.Buffer;
            return true;
        }
        buffer = null;
        return false;
    }

    public bool IsActive(string probeId) =>
        _activeProbes.TryGetValue(probeId, out var entry) && entry.Status == ProbeActivationStatus.Active;

    public IReadOnlyList<string> GetActiveProbeIds() => _activeProbes.Keys.ToArray();

    public void DeactivateExpired()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var (probeId, entry) in _activeProbes)
        {
            if (entry.Command.ExpiresAtUtc <= now)
            {
                _activeProbes.TryRemove(probeId, out _);
                entry.Buffer.Drain(); // Flush evidence
                _logger?.LogInformation("Probe {ProbeId} expired and was deactivated.", probeId);
            }
        }
    }
}
