namespace RuntimeInvestigation.Domain.Entities;

public sealed record ProbeTemplate(
    ProbeType Type,
    string TargetClass,
    string TargetMethod,
    string? Expression = null,
    string? Condition = null,
    int DurationMinutes = 30);
