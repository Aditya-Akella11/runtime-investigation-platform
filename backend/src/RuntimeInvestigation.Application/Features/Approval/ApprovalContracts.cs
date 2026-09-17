using RuntimeInvestigation.Application.Features.Investigations;

namespace RuntimeInvestigation.Application.Features.Approval;

public sealed record SubmitApprovalCommand(string InvestigationId);

public sealed record ApproveInvestigationCommand(string InvestigationId, string? AdminId = null);

public sealed record RejectInvestigationCommand(string InvestigationId, string Reason, string? AdminId = null);
