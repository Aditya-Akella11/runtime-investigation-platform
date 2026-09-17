using System;
using System.Threading;
using System.Threading.Tasks;
using RuntimeInvestigation.Application.Common.Interfaces;
using RuntimeInvestigation.Application.Features.Investigations;
using RuntimeInvestigation.Domain.Entities;
using RuntimeInvestigation.Shared.Results;

namespace RuntimeInvestigation.Application.Features.Approval;

public class ApprovalService
{
    private readonly IInvestigationRepository _repository;
    private readonly ITenantContext? _tenantContext;

    public ApprovalService(IInvestigationRepository repository, ITenantContext? tenantContext = null)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<InvestigationDto>> SubmitForApprovalAsync(SubmitApprovalCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.InvestigationId))
        {
            return Result<InvestigationDto>.Failure(new Error("ValidationError", "InvestigationId is required."));
        }

        var entity = await _repository.GetByIdAsync(command.InvestigationId, cancellationToken);
        if (entity is null || (_tenantContext != null && !string.Equals(entity.TenantId, _tenantContext.TenantId, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<InvestigationDto>.Failure(new Error("NotFound", $"Investigation '{command.InvestigationId}' not found."));
        }

        try
        {
            entity.SubmitForApproval();
            await _repository.UpdateAsync(entity, cancellationToken);
            return Result<InvestigationDto>.Success(InvestigationDto.FromEntity(entity));
        }
        catch (InvalidOperationException ex)
        {
            return Result<InvestigationDto>.Failure(new Error("InvalidTransition", ex.Message));
        }
        catch (Exception ex)
        {
            return Result<InvestigationDto>.Failure(new Error("UpdateFailed", ex.Message));
        }
    }

    public async Task<Result<InvestigationDto>> ApproveAsync(ApproveInvestigationCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.InvestigationId))
        {
            return Result<InvestigationDto>.Failure(new Error("ValidationError", "InvestigationId is required."));
        }

        if (_tenantContext != null && !string.Equals(_tenantContext.Role, nameof(UserRole.Admin), StringComparison.OrdinalIgnoreCase))
        {
            return Result<InvestigationDto>.Failure(new Error("Forbidden", "Only Admin users can approve investigations."));
        }

        var entity = await _repository.GetByIdAsync(command.InvestigationId, cancellationToken);
        if (entity is null || (_tenantContext != null && !string.Equals(entity.TenantId, _tenantContext.TenantId, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<InvestigationDto>.Failure(new Error("NotFound", $"Investigation '{command.InvestigationId}' not found."));
        }

        var adminId = !string.IsNullOrWhiteSpace(command.AdminId)
            ? command.AdminId
            : (_tenantContext?.UserId ?? "system");

        try
        {
            entity.Approve(adminId);
            await _repository.UpdateAsync(entity, cancellationToken);
            return Result<InvestigationDto>.Success(InvestigationDto.FromEntity(entity));
        }
        catch (InvalidOperationException ex)
        {
            return Result<InvestigationDto>.Failure(new Error("InvalidTransition", ex.Message));
        }
        catch (Exception ex)
        {
            return Result<InvestigationDto>.Failure(new Error("UpdateFailed", ex.Message));
        }
    }

    public async Task<Result<InvestigationDto>> RejectAsync(RejectInvestigationCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.InvestigationId))
        {
            return Result<InvestigationDto>.Failure(new Error("ValidationError", "InvestigationId is required."));
        }

        if (string.IsNullOrWhiteSpace(command.Reason))
        {
            return Result<InvestigationDto>.Failure(new Error("ValidationError", "Rejection reason is required."));
        }

        if (_tenantContext != null && !string.Equals(_tenantContext.Role, nameof(UserRole.Admin), StringComparison.OrdinalIgnoreCase))
        {
            return Result<InvestigationDto>.Failure(new Error("Forbidden", "Only Admin users can reject investigations."));
        }

        var entity = await _repository.GetByIdAsync(command.InvestigationId, cancellationToken);
        if (entity is null || (_tenantContext != null && !string.Equals(entity.TenantId, _tenantContext.TenantId, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<InvestigationDto>.Failure(new Error("NotFound", $"Investigation '{command.InvestigationId}' not found."));
        }

        var adminId = !string.IsNullOrWhiteSpace(command.AdminId)
            ? command.AdminId
            : (_tenantContext?.UserId ?? "system");

        try
        {
            entity.Reject(adminId, command.Reason);
            await _repository.UpdateAsync(entity, cancellationToken);
            return Result<InvestigationDto>.Success(InvestigationDto.FromEntity(entity));
        }
        catch (InvalidOperationException ex)
        {
            return Result<InvestigationDto>.Failure(new Error("InvalidTransition", ex.Message));
        }
        catch (Exception ex)
        {
            return Result<InvestigationDto>.Failure(new Error("UpdateFailed", ex.Message));
        }
    }
}
