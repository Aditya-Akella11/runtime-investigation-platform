using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RuntimeInvestigation.Domain.Entities;

namespace RuntimeInvestigation.Application.Features.Probes;

public interface IProbeResultRepository
{
    Task AddAsync(ProbeResult result, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<ProbeResult> results, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProbeResult>> GetByProbeIdAsync(string probeId, int limit = 100, CancellationToken cancellationToken = default);
    Task<long> DeleteOlderThanAsync(DateTime thresholdUtc, CancellationToken cancellationToken = default);
}
