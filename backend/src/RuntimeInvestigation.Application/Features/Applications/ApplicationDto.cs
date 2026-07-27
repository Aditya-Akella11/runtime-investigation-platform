namespace RuntimeInvestigation.Application.Features.Applications;

public sealed record ApplicationDto(string Id, string Name, string? Description, DateTime CreatedAt);
