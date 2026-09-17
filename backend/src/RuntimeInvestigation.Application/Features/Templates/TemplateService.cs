using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RuntimeInvestigation.Application.Common.Interfaces;
using RuntimeInvestigation.Application.Features.Investigations;
using RuntimeInvestigation.Application.Features.Probes;
using RuntimeInvestigation.Domain.Entities;
using RuntimeInvestigation.Shared.Results;

namespace RuntimeInvestigation.Application.Features.Templates;

public class TemplateService
{
    private readonly ITemplateRepository _templateRepository;
    private readonly InvestigationService _investigationService;
    private readonly ProbeService _probeService;
    private readonly ITenantContext? _tenantContext;

    public TemplateService(
        ITemplateRepository templateRepository,
        InvestigationService investigationService,
        ProbeService probeService,
        ITenantContext? tenantContext = null)
    {
        _templateRepository = templateRepository;
        _investigationService = investigationService;
        _probeService = probeService;
        _tenantContext = tenantContext;
    }

    public async Task<IReadOnlyList<InvestigationTemplateDto>> GetTemplatesAsync(CancellationToken cancellationToken = default)
    {
        var templates = await _templateRepository.GetAllAsync(cancellationToken);
        return templates.Select(InvestigationTemplateDto.FromEntity).ToList();
    }

    public async Task<InvestigationTemplateDto?> GetTemplateByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var template = await _templateRepository.GetByIdAsync(id, cancellationToken);
        return template == null ? null : InvestigationTemplateDto.FromEntity(template);
    }

    public async Task<Result<TemplateInstantiationResult>> CreateFromTemplateAsync(CreateFromTemplateCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.TemplateId))
        {
            return Result<TemplateInstantiationResult>.Failure(new Error("ValidationError", "TemplateId is required."));
        }
        if (string.IsNullOrWhiteSpace(command.ApplicationId))
        {
            return Result<TemplateInstantiationResult>.Failure(new Error("ValidationError", "ApplicationId is required."));
        }

        var template = await _templateRepository.GetByIdAsync(command.TemplateId, cancellationToken);
        if (template == null)
        {
            return Result<TemplateInstantiationResult>.Failure(new Error("NotFound", $"Template '{command.TemplateId}' not found."));
        }

        var title = !string.IsNullOrWhiteSpace(command.Title) ? command.Title : template.Name;
        var description = !string.IsNullOrWhiteSpace(command.Description) ? command.Description : template.Description;

        var invResult = await _investigationService.CreateAsync(new CreateInvestigationCommand(
            command.ApplicationId,
            title,
            description), cancellationToken);

        if (!invResult.IsSuccess || invResult.Value == null)
        {
            return Result<TemplateInstantiationResult>.Failure(invResult.Error ?? new Error("CreateFailed", "Failed to create investigation from template."));
        }

        var createdProbes = new List<ProbeDto>();
        foreach (var pt in template.ProbeTemplates)
        {
            // Deep copy probe specifications from template
            var addProbeCmd = new AddProbeCommand(
                invResult.Value.Id,
                pt.Type.ToString(),
                pt.TargetClass,
                pt.TargetMethod,
                pt.Expression != null ? new string(pt.Expression) : null,
                pt.DurationMinutes,
                pt.Condition != null ? new string(pt.Condition) : null);

            var probeResult = await _probeService.AddProbeAsync(addProbeCmd, cancellationToken);
            if (probeResult.IsSuccess && probeResult.Value != null)
            {
                createdProbes.Add(probeResult.Value);
            }
        }

        return Result<TemplateInstantiationResult>.Success(new TemplateInstantiationResult(invResult.Value, createdProbes));
    }
}
