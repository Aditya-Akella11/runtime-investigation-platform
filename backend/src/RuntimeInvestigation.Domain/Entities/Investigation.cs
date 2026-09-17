namespace RuntimeInvestigation.Domain.Entities;

public enum InvestigationStatus { Open, Investigating, Resolved, Closed }

public class Investigation
{
    protected Investigation() { }

    public Investigation(string applicationId, string title, string? description = null, string tenantId = "default", string createdBy = "system")
    {
        if (string.IsNullOrWhiteSpace(applicationId)) throw new ArgumentException("Application is required.", nameof(applicationId));
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(title));
        Id = Guid.NewGuid().ToString("N");
        ApplicationId = applicationId.Trim();
        Title = title.Trim();
        Description = description?.Trim();
        TenantId = string.IsNullOrWhiteSpace(tenantId) ? "default" : tenantId.Trim();
        CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "system" : createdBy.Trim();
        Status = InvestigationStatus.Open;
        CreatedAt = UpdatedAt = DateTime.UtcNow;
    }

    public string Id { get; private set; } = string.Empty;
    public string TenantId { get; set; } = "default";
    public string CreatedBy { get; set; } = "system";
    public string ApplicationId { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public InvestigationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public ApprovalStatus ApprovalStatus { get; private set; } = ApprovalStatus.Draft;
    public string? ApprovedBy { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime? ApprovedAt { get; private set; }

    public void SubmitForApproval()
    {
        if (ApprovalStatus != ApprovalStatus.Draft && ApprovalStatus != ApprovalStatus.Rejected)
        {
            throw new InvalidOperationException($"Cannot submit investigation for approval from {ApprovalStatus} status.");
        }
        ApprovalStatus = ApprovalStatus.PendingApproval;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve(string adminId)
    {
        if (string.IsNullOrWhiteSpace(adminId)) throw new ArgumentException("AdminId is required for approval.", nameof(adminId));
        if (ApprovalStatus != ApprovalStatus.PendingApproval)
        {
            throw new InvalidOperationException($"Cannot approve investigation in {ApprovalStatus} status.");
        }
        ApprovalStatus = ApprovalStatus.Active;
        ApprovedBy = adminId.Trim();
        ApprovedAt = DateTime.UtcNow;
        Status = InvestigationStatus.Investigating;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(string adminId, string reason)
    {
        if (string.IsNullOrWhiteSpace(adminId)) throw new ArgumentException("AdminId is required for rejection.", nameof(adminId));
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Rejection reason is required.", nameof(reason));
        if (ApprovalStatus != ApprovalStatus.PendingApproval)
        {
            throw new InvalidOperationException($"Cannot reject investigation in {ApprovalStatus} status.");
        }
        ApprovalStatus = ApprovalStatus.Rejected;
        ApprovedBy = adminId.Trim();
        RejectionReason = reason.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string title, string? description, InvestigationStatus status)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.", nameof(title));
        Title = title.Trim();
        Description = description?.Trim();
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Start() => TransitionTo(InvestigationStatus.Investigating);
    public void Resolve() => TransitionTo(InvestigationStatus.Resolved);
    public void Close() => TransitionTo(InvestigationStatus.Closed);

    private void TransitionTo(InvestigationStatus next)
    {
        var allowed = (Status, next) switch
        {
            (InvestigationStatus.Open, InvestigationStatus.Investigating) => true,
            (InvestigationStatus.Investigating, InvestigationStatus.Resolved) => true,
            (InvestigationStatus.Resolved, InvestigationStatus.Closed) => true,
            _ => false
        };
        if (!allowed) throw new InvalidOperationException($"Cannot transition investigation from {Status} to {next}.");
        Status = next; UpdatedAt = DateTime.UtcNow;
    }
}

public enum ApprovalStatus
{
    Draft,
    PendingApproval,
    Active,
    Rejected
}
