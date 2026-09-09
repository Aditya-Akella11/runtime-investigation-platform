using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RuntimeInvestigation.Application.Common.Interfaces;
using RuntimeInvestigation.Domain.Entities;
using RuntimeInvestigation.Shared.Results;

namespace RuntimeInvestigation.Application.Features.Investigations;

public sealed class InvestigationService
{
    private readonly IInvestigationRepository _repository;
    private readonly ITenantContext? _tenantContext;

    public InvestigationService(IInvestigationRepository repository, ITenantContext? tenantContext = null)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<Result<InvestigationDto>> CreateAsync(CreateInvestigationCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.ApplicationId))
        {
            return Result<InvestigationDto>.Failure(new Error("ValidationError", "ApplicationId is required."));
        }

        if (string.IsNullOrWhiteSpace(command.Title))
        {
            return Result<InvestigationDto>.Failure(new Error("ValidationError", "Title is required."));
        }

        try
        {
            var tenantId = !string.IsNullOrWhiteSpace(command.TenantId)
                ? command.TenantId
                : (_tenantContext?.TenantId ?? "default");
            var createdBy = !string.IsNullOrWhiteSpace(command.CreatedBy)
                ? command.CreatedBy
                : (_tenantContext?.UserId ?? "system");

            var entity = new Investigation(command.ApplicationId, command.Title, command.Description, tenantId, createdBy);
            await _repository.CreateAsync(entity, cancellationToken);
            return Result<InvestigationDto>.Success(InvestigationDto.FromEntity(entity));
        }
        catch (Exception ex)
        {
            return Result<InvestigationDto>.Failure(new Error("CreateFailed", ex.Message));
        }
    }

    public async Task<Result<InvestigationDto>> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return Result<InvestigationDto>.Failure(new Error("ValidationError", "Id is required."));
        }

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null || (_tenantContext != null && !string.Equals(entity.TenantId, _tenantContext.TenantId, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<InvestigationDto>.Failure(new Error("NotFound", $"Investigation '{id}' not found."));
        }

        return Result<InvestigationDto>.Success(InvestigationDto.FromEntity(entity));
    }

    public async Task<IReadOnlyList<InvestigationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);
        if (_tenantContext != null)
        {
            entities = entities.Where(e => string.Equals(e.TenantId, _tenantContext.TenantId, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        return entities.Select(InvestigationDto.FromEntity).ToArray();
    }

    public async Task<IReadOnlyList<InvestigationDto>> GetByApplicationIdAsync(string applicationId, CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetByApplicationIdAsync(applicationId, cancellationToken);
        if (_tenantContext != null)
        {
            entities = entities.Where(e => string.Equals(e.TenantId, _tenantContext.TenantId, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        return entities.Select(InvestigationDto.FromEntity).ToArray();
    }

    public async Task<Result<InvestigationDto>> UpdateAsync(string id, UpdateInvestigationCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null || (_tenantContext != null && !string.Equals(entity.TenantId, _tenantContext.TenantId, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<InvestigationDto>.Failure(new Error("NotFound", $"Investigation '{id}' not found."));
        }

        if (string.IsNullOrWhiteSpace(command.Title))
        {
            return Result<InvestigationDto>.Failure(new Error("ValidationError", "Title is required."));
        }

        InvestigationStatus status = entity.Status;
        if (!string.IsNullOrWhiteSpace(command.Status) && Enum.TryParse<InvestigationStatus>(command.Status, true, out var parsedStatus))
        {
            status = parsedStatus;
        }

        entity.Update(command.Title, command.Description, status);
        await _repository.UpdateAsync(entity, cancellationToken);
        return Result<InvestigationDto>.Success(InvestigationDto.FromEntity(entity));
    }

    public async Task<Result<InvestigationDto>> StartAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null || (_tenantContext != null && !string.Equals(entity.TenantId, _tenantContext.TenantId, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<InvestigationDto>.Failure(new Error("NotFound", $"Investigation '{id}' not found."));
        }

        try
        {
            entity.Start();
            await _repository.UpdateAsync(entity, cancellationToken);
            return Result<InvestigationDto>.Success(InvestigationDto.FromEntity(entity));
        }
        catch (Exception ex)
        {
            return Result<InvestigationDto>.Failure(new Error("InvalidTransition", ex.Message));
        }
    }

    public async Task<Result<InvestigationDto>> ResolveAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null || (_tenantContext != null && !string.Equals(entity.TenantId, _tenantContext.TenantId, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<InvestigationDto>.Failure(new Error("NotFound", $"Investigation '{id}' not found."));
        }

        try
        {
            entity.Resolve();
            await _repository.UpdateAsync(entity, cancellationToken);
            return Result<InvestigationDto>.Success(InvestigationDto.FromEntity(entity));
        }
        catch (Exception ex)
        {
            return Result<InvestigationDto>.Failure(new Error("InvalidTransition", ex.Message));
        }
    }

    public async Task<Result<InvestigationDto>> CloseAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null || (_tenantContext != null && !string.Equals(entity.TenantId, _tenantContext.TenantId, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<InvestigationDto>.Failure(new Error("NotFound", $"Investigation '{id}' not found."));
        }

        try
        {
            entity.Close();
            await _repository.UpdateAsync(entity, cancellationToken);
            return Result<InvestigationDto>.Success(InvestigationDto.FromEntity(entity));
        }
        catch (Exception ex)
        {
            return Result<InvestigationDto>.Failure(new Error("InvalidTransition", ex.Message));
        }
    }

    public async Task<Result<bool>> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null || (_tenantContext != null && !string.Equals(entity.TenantId, _tenantContext.TenantId, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<bool>.Failure(new Error("NotFound", $"Investigation '{id}' not found."));
        }

        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        return deleted
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(new Error("NotFound", $"Investigation '{id}' not found."));
    }
}
