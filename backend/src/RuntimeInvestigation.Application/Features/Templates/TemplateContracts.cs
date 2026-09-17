using System;
using System.Collections.Generic;
using System.Linq;
using RuntimeInvestigation.Application.Features.Investigations;
using RuntimeInvestigation.Application.Features.Probes;
using RuntimeInvestigation.Domain.Entities;

namespace RuntimeInvestigation.Application.Features.Templates;

public sealed record ProbeTemplateDto(
    string Type,
    string TargetClass,
    string TargetMethod,
    string? Expression = null,
    string? Condition = null,
    int DurationMinutes = 30)
{
    public static ProbeTemplateDto FromEntity(ProbeTemplate entity) =>
        new(
            entity.Type.ToString(),
            entity.TargetClass,
            entity.TargetMethod,
            entity.Expression,
            entity.Condition,
            entity.DurationMinutes);
}

public sealed record InvestigationTemplateDto(
    string Id,
    string Name,
    string Description,
    string Category,
    IReadOnlyList<ProbeTemplateDto> ProbeTemplates)
{
    public static InvestigationTemplateDto FromEntity(InvestigationTemplate entity) =>
        new(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.Category,
            entity.ProbeTemplates.Select(ProbeTemplateDto.FromEntity).ToList());
}

public sealed record CreateFromTemplateCommand(
    string TemplateId,
    string ApplicationId,
    string? Title = null,
    string? Description = null);

public sealed record TemplateInstantiationResult(
    InvestigationDto Investigation,
    IReadOnlyList<ProbeDto> Probes);
