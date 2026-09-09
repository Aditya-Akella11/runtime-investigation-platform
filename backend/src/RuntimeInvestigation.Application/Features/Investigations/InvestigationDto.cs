using RuntimeInvestigation.Domain.Entities;

namespace RuntimeInvestigation.Application.Features.Investigations;

public sealed record InvestigationDto(
    string Id,
    string ApplicationId,
    string Title,
    string? Description,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string TenantId = "default",
    string CreatedBy = "system")
{
    public static InvestigationDto FromEntity(Investigation entity) =>
        new(
            entity.Id,
            entity.ApplicationId,
            entity.Title,
            entity.Description,
            entity.Status.ToString(),
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.TenantId,
            entity.CreatedBy);
}

public sealed record CreateInvestigationCommand(
    string ApplicationId,
    string Title,
    string? Description = null,
    string? TenantId = null,
    string? CreatedBy = null);

public sealed record UpdateInvestigationCommand(
    string Title,
    string? Description = null,
    string? Status = null);
