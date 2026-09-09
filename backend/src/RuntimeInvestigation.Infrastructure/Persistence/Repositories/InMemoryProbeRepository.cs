using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RuntimeInvestigation.Application.Features.Probes;
using RuntimeInvestigation.Domain.Entities;

namespace RuntimeInvestigation.Infrastructure.Persistence.Repositories;

public sealed class InMemoryProbeRepository : IProbeRepository
{
    private readonly ConcurrentDictionary<string, RuntimeProbe> _probes = new();
    public RuntimeInvestigation.Application.Common.Interfaces.ITenantContext? TenantContext { get; set; }

    public InMemoryProbeRepository() { }

    public Task<RuntimeProbe?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _probes.TryGetValue(id, out var probe);
        if (probe != null && TenantContext != null && !string.Equals(probe.TenantId, TenantContext.TenantId, StringComparison.OrdinalIgnoreCase))
        {
            probe = null;
        }
        return Task.FromResult(probe);
    }

    public Task<IReadOnlyList<RuntimeProbe>> GetByInvestigationIdAsync(string investigationId, CancellationToken cancellationToken = default)
    {
        var list = _probes.Values
            .Where(p => p.InvestigationId == investigationId && (TenantContext == null || string.Equals(p.TenantId, TenantContext.TenantId, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(p => p.CreatedAt)
            .ToArray();
        return Task.FromResult<IReadOnlyList<RuntimeProbe>>(list);
    }

    public Task<IReadOnlyList<RuntimeProbe>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = _probes.Values
            .Where(p => TenantContext == null || string.Equals(p.TenantId, TenantContext.TenantId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(p => p.CreatedAt)
            .ToArray();
        return Task.FromResult<IReadOnlyList<RuntimeProbe>>(list);
    }

    public Task<IReadOnlyList<RuntimeProbe>> GetActiveExpiredAsync(DateTime utcNow, CancellationToken cancellationToken = default)
    {
        var list = _probes.Values
            .Where(p => p.Status == ProbeStatus.Active && p.ExpiresAt <= utcNow)
            .ToArray();
        return Task.FromResult<IReadOnlyList<RuntimeProbe>>(list);
    }

    public Task CreateAsync(RuntimeProbe probe, CancellationToken cancellationToken = default)
    {
        _probes[probe.Id] = probe;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(RuntimeProbe probe, CancellationToken cancellationToken = default)
    {
        _probes[probe.Id] = probe;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_probes.TryRemove(id, out _));
    }

    public void Clear() => _probes.Clear();
}

public sealed class InMemoryProbeResultRepository : IProbeResultRepository
{
    private readonly ConcurrentBag<ProbeResult> _results = new();

    public Task AddAsync(ProbeResult result, CancellationToken cancellationToken = default)
    {
        _results.Add(result);
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<ProbeResult> results, CancellationToken cancellationToken = default)
    {
        foreach (var r in results)
        {
            _results.Add(r);
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ProbeResult>> GetByProbeIdAsync(string probeId, int limit = 100, CancellationToken cancellationToken = default)
    {
        var list = _results
            .Where(r => r.ProbeId == probeId)
            .OrderByDescending(r => r.CapturedAtUtc)
            .Take(limit)
            .ToArray();
        return Task.FromResult<IReadOnlyList<ProbeResult>>(list);
    }

    public Task<long> DeleteOlderThanAsync(DateTime thresholdUtc, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(0L);
    }

    public void Clear() => _results.Clear();
}
