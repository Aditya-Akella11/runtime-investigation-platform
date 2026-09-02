using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RuntimeInvestigation.Shared.Contracts;

namespace RuntimeInvestigation.RuntimeAgent.Instrumentation;

public sealed record IngestEvidenceRequest(
    string ProbeId,
    IReadOnlyList<ProbeEvidenceRecord> Records);

public sealed class EvidenceUploader : BackgroundService
{
    private readonly ProbeActivator _probeActivator;
    private readonly HttpClient _httpClient;
    private readonly ILogger<EvidenceUploader> _logger;
    private readonly TimeSpan _flushInterval;

    public EvidenceUploader(
        ProbeActivator probeActivator,
        HttpClient httpClient,
        ILogger<EvidenceUploader> logger,
        TimeSpan? flushInterval = null)
    {
        _probeActivator = probeActivator;
        _httpClient = httpClient;
        _logger = logger;
        _flushInterval = flushInterval ?? TimeSpan.FromSeconds(2);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_flushInterval, stoppingToken);
                await FlushAllBuffersAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error occurred during background evidence upload.");
            }
        }
    }

    public async Task FlushAllBuffersAsync(CancellationToken cancellationToken)
    {
        var activeProbeIds = _probeActivator.GetActiveProbeIds();
        foreach (var probeId in activeProbeIds)
        {
            if (_probeActivator.TryGetBuffer(probeId, out var buffer) && buffer != null)
            {
                var entries = buffer.Drain();
                if (entries.Count == 0) continue;

                var records = entries.Select(e => new ProbeEvidenceRecord(
                    e.ProbeId,
                    Guid.NewGuid().ToString("N"),
                    e.Arguments,
                    e.TimestampUtc,
                    AgentProtocol.Version,
                    e.MethodName,
                    null
                )).ToArray();

                try
                {
                    var response = await _httpClient.PostAsJsonAsync($"/api/probes/{probeId}/evidence", records, cancellationToken);
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Evidence upload for probe {ProbeId} returned status {Status}", probeId, response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to upload evidence batch for probe {ProbeId}", probeId);
                }
            }
        }
    }
}
