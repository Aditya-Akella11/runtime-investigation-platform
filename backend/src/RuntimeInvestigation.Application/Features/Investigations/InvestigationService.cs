using RuntimeInvestigation.Domain.Entities;
using RuntimeInvestigation.Shared.Results;

namespace RuntimeInvestigation.Application.Features.Investigations;

public sealed class InvestigationService
{
    private readonly IInvestigationRepository _repository;

    public InvestigationService(IInvestigationRepository repository)
    {
        _repository = repository;
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
            var entity = new Investigation(command.ApplicationId, command.Title, command.Description);
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
        if (entity is null)
        {
            return Result<InvestigationDto>.Failure(new Error("NotFound", $"Investigation '{id}' not found."));
        }

        return Result<InvestigationDto>.Success(InvestigationDto.FromEntity(entity));
    }

    public async Task<IReadOnlyList<InvestigationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);
        return entities.Select(InvestigationDto.FromEntity).ToArray();
    }

    public async Task<IReadOnlyList<InvestigationDto>> GetByApplicationIdAsync(string applicationId, CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetByApplicationIdAsync(applicationId, cancellationToken);
        return entities.Select(InvestigationDto.FromEntity).ToArray();
    }

    public async Task<Result<InvestigationDto>> UpdateAsync(string id, UpdateInvestigationCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
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
        if (entity is null)
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
        if (entity is null)
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
        if (entity is null)
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
        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        return deleted
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(new Error("NotFound", $"Investigation '{id}' not found."));
    }
}
