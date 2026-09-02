using System;
using System.Collections.Generic;
using RuntimeInvestigation.Domain.Entities;

namespace RuntimeInvestigation.Application.Features.Probes;

public sealed record AddProbeCommand(
    string InvestigationId,
    string ProbeType,
    string TargetClass,
    string TargetMethod,
    string? Expression = null,
    int DurationMinutes = 30);

public sealed record ActivateProbeCommand(string ProbeId);

public sealed record DeactivateProbeCommand(string ProbeId, string Reason = "Operator deactivated");

public sealed record ProbeDto(
    string Id,
    string InvestigationId,
    string Type,
    string Target,
    string? Expression,
    string Status,
    DateTime CreatedAt,
    DateTime ExpiresAt)
{
    public static ProbeDto FromEntity(RuntimeProbe entity) =>
        new(
            entity.Id,
            entity.InvestigationId,
            entity.Type.ToString(),
            entity.Target,
            entity.Expression,
            entity.Status.ToString(),
            entity.CreatedAt,
            entity.ExpiresAt);
}

public sealed record ProbeResultDto(
    string Id,
    string ProbeId,
    string CorrelationId,
    string MethodName,
    IReadOnlyDictionary<string, string> Arguments,
    string? ReturnValue,
    double DurationMs,
    DateTime CapturedAtUtc)
{
    public static ProbeResultDto FromEntity(ProbeResult entity) =>
        new(
            entity.Id,
            entity.ProbeId,
            entity.CorrelationId,
            entity.MethodName,
            entity.Arguments,
            entity.ReturnValue,
            entity.DurationMs,
            entity.CapturedAtUtc);
}
