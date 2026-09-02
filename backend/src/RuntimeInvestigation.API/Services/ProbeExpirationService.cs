using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RuntimeInvestigation.Application.Features.Probes;

namespace RuntimeInvestigation.API.Services;

public sealed class ProbeExpirationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ProbeExpirationService> _logger;
    private readonly TimeSpan _checkInterval;

    public ProbeExpirationService(
        IServiceScopeFactory scopeFactory,
        ILogger<ProbeExpirationService> logger,
        TimeSpan? checkInterval = null)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _checkInterval = checkInterval ?? TimeSpan.FromSeconds(5);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ProbeExpirationService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_checkInterval, stoppingToken);
                await CheckAndExpireProbesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while checking for expired probes.");
            }
        }

        _logger.LogInformation("ProbeExpirationService stopped.");
    }

    public async Task CheckAndExpireProbesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var probeRepository = scope.ServiceProvider.GetRequiredService<IProbeRepository>();
        var probeService = scope.ServiceProvider.GetRequiredService<ProbeService>();

        var expiredProbes = await probeRepository.GetActiveExpiredAsync(DateTime.UtcNow, cancellationToken);
        foreach (var probe in expiredProbes)
        {
            _logger.LogInformation("Probe {ProbeId} has expired. Deactivating and cleaning up...", probe.Id);
            try
            {
                await probeService.DeactivateProbeAsync(probe.Id, "Probe expired automatically", cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clean up expired probe {ProbeId}", probe.Id);
            }
        }
    }
}
