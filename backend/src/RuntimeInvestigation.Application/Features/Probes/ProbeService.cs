using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RuntimeInvestigation.Domain.Entities;
using RuntimeInvestigation.Shared.Contracts;
using RuntimeInvestigation.Shared.Results;

namespace RuntimeInvestigation.Application.Features.Probes;

public interface IAgentDispatcher
{
    Task<ProbeActivationAck> DispatchActivationAsync(ProbeActivationCommand command, CancellationToken cancellationToken = default);
    Task<ProbeRemovalAck> DispatchRemovalAsync(ProbeRemovalCommand command, CancellationToken cancellationToken = default);
}

public sealed class ProbeService
{
    private readonly IProbeRepository _probeRepository;
    private readonly IProbeResultRepository _resultRepository;
    private readonly IAgentDispatcher _dispatcher;
    private readonly RuntimeInvestigation.Application.Common.Interfaces.ITenantContext? _tenantContext;

    public ProbeService(
        IProbeRepository probeRepository,
        IProbeResultRepository resultRepository,
        IAgentDispatcher dispatcher,
        RuntimeInvestigation.Application.Common.Interfaces.ITenantContext? tenantContext = null)
    {
        _probeRepository = probeRepository;
        _resultRepository = resultRepository;
        _dispatcher = dispatcher;
        _tenantContext = tenantContext;
    }

    public async Task<Result<ProbeDto>> AddProbeAsync(AddProbeCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.InvestigationId))
        {
            return Result<ProbeDto>.Failure(new Error("ValidationError", "InvestigationId is required."));
        }

        if (string.IsNullOrWhiteSpace(command.TargetClass) || string.IsNullOrWhiteSpace(command.TargetMethod))
        {
            return Result<ProbeDto>.Failure(new Error("ValidationError", "TargetClass and TargetMethod are required."));
        }

        if (!Enum.TryParse<ProbeType>(command.ProbeType, true, out var parsedType))
        {
            parsedType = ProbeType.Log;
        }

        int duration = Math.Clamp(command.DurationMinutes <= 0 ? 30 : command.DurationMinutes, 1, 120);
        var expiresAt = DateTime.UtcNow.AddMinutes(duration);
        var target = $"{command.TargetClass.Trim()}.{command.TargetMethod.Trim()}";

        try
        {
            var tenantId = _tenantContext?.TenantId ?? "default";
            var probe = new RuntimeProbe(command.InvestigationId, parsedType, target, command.Expression, expiresAt, command.Condition, tenantId);
            await _probeRepository.CreateAsync(probe, cancellationToken);
            return Result<ProbeDto>.Success(ProbeDto.FromEntity(probe));
        }
        catch (Exception ex)
        {
            return Result<ProbeDto>.Failure(new Error("CreateFailed", ex.Message));
        }
    }

    public async Task<Result<ProbeDto>> ActivateProbeAsync(string probeId, string applicationName = "DefaultApp", CancellationToken cancellationToken = default)
    {
        var probe = await _probeRepository.GetByIdAsync(probeId, cancellationToken);
        if (probe is null || (_tenantContext != null && !string.Equals(probe.TenantId, _tenantContext.TenantId, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<ProbeDto>.Failure(new Error("NotFound", $"Probe '{probeId}' not found."));
        }

        int lastDot = probe.Target.LastIndexOf('.');
        string targetClass = lastDot > 0 ? probe.Target[..lastDot] : probe.Target;
        string targetMethod = lastDot > 0 ? probe.Target[(lastDot + 1)..] : probe.Target;

        var command = new ProbeActivationCommand(
            probe.Id,
            applicationName,
            targetClass,
            targetMethod,
            new DateTimeOffset(probe.ExpiresAt),
            AgentProtocol.Version,
            probe.Expression);

        try
        {
            var ack = await _dispatcher.DispatchActivationAsync(command, cancellationToken);
            if (!ack.Success)
            {
                probe.Fail();
                await _probeRepository.UpdateAsync(probe, cancellationToken);
                return Result<ProbeDto>.Failure(new Error("DispatchFailed", ack.ErrorMessage ?? "Agent rejected probe activation."));
            }

            probe.Activate();
            await _probeRepository.UpdateAsync(probe, cancellationToken);
            return Result<ProbeDto>.Success(ProbeDto.FromEntity(probe));
        }
        catch (Exception ex)
        {
            probe.Fail();
            await _probeRepository.UpdateAsync(probe, cancellationToken);
            return Result<ProbeDto>.Failure(new Error("ActivationError", ex.Message));
        }
    }

    public async Task<Result<ProbeDto>> DeactivateProbeAsync(string probeId, string reason = "Operator deactivated", CancellationToken cancellationToken = default)
    {
        var probe = await _probeRepository.GetByIdAsync(probeId, cancellationToken);
        if (probe is null || (_tenantContext != null && !string.Equals(probe.TenantId, _tenantContext.TenantId, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<ProbeDto>.Failure(new Error("NotFound", $"Probe '{probeId}' not found."));
        }

        if (probe.Status == ProbeStatus.Removed || probe.Status == ProbeStatus.Completed)
        {
            return Result<ProbeDto>.Success(ProbeDto.FromEntity(probe)); // Idempotent
        }

        var command = new ProbeRemovalCommand(probe.Id, reason, AgentProtocol.Version);

        try
        {
            await _dispatcher.DispatchRemovalAsync(command, cancellationToken);
        }
        catch
        {
            // Even if network fails, mark removed locally to prevent zombie probes
        }

        probe.Remove();
        await _probeRepository.UpdateAsync(probe, cancellationToken);
        return Result<ProbeDto>.Success(ProbeDto.FromEntity(probe));
    }

    public async Task<Result<ProbeDto>> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var probe = await _probeRepository.GetByIdAsync(id, cancellationToken);
        if (probe is null || (_tenantContext != null && !string.Equals(probe.TenantId, _tenantContext.TenantId, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<ProbeDto>.Failure(new Error("NotFound", $"Probe '{id}' not found."));
        }
        return Result<ProbeDto>.Success(ProbeDto.FromEntity(probe));
    }

    public async Task<IReadOnlyList<ProbeDto>> GetByInvestigationIdAsync(string investigationId, CancellationToken cancellationToken = default)
    {
        var probes = await _probeRepository.GetByInvestigationIdAsync(investigationId, cancellationToken);
        if (_tenantContext != null)
        {
            probes = probes.Where(p => string.Equals(p.TenantId, _tenantContext.TenantId, StringComparison.OrdinalIgnoreCase)).ToArray();
        }
        return probes.Select(ProbeDto.FromEntity).ToArray();
    }

    public async Task<IReadOnlyList<ProbeResultDto>> GetResultsAsync(string probeId, int limit = 100, CancellationToken cancellationToken = default)
    {
        var boundedLimit = Math.Clamp(limit, 1, 500);
        var results = await _resultRepository.GetByProbeIdAsync(probeId, boundedLimit, cancellationToken);
        return results.Select(ProbeResultDto.FromEntity).ToArray();
    }
}
