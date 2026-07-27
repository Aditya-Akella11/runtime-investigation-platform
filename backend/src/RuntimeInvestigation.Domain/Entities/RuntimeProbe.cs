namespace RuntimeInvestigation.Domain.Entities;

public enum ProbeType { Log, Snapshot, Metric, Trace }
public enum ProbeStatus { Pending, Active, Completed, Failed, Expired, Removed }

public class RuntimeProbe
{
    protected RuntimeProbe() { }

    public RuntimeProbe(string investigationId, ProbeType type, string target, string? expression, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(investigationId)) throw new ArgumentException("Investigation is required.", nameof(investigationId));
        if (string.IsNullOrWhiteSpace(target)) throw new ArgumentException("Target is required.", nameof(target));
        if (expiresAt <= DateTime.UtcNow) throw new ArgumentException("Expiry must be in the future.", nameof(expiresAt));
        Id = Guid.NewGuid().ToString("N");
        InvestigationId = investigationId.Trim();
        Type = type;
        Target = target.Trim();
        Expression = expression?.Trim();
        Status = ProbeStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt.ToUniversalTime();
    }

    public string Id { get; private set; } = string.Empty;
    public string InvestigationId { get; private set; } = string.Empty;
    public ProbeType Type { get; private set; }
    public string Target { get; private set; } = string.Empty;
    public string? Expression { get; private set; }
    public ProbeStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    public void Activate()
    {
        EnsureNotTerminal();
        if (DateTime.UtcNow >= ExpiresAt) { Status = ProbeStatus.Expired; throw new InvalidOperationException("An expired probe cannot be activated."); }
        Status = ProbeStatus.Active;
    }
    public void Complete() { EnsureNotTerminal(); Status = ProbeStatus.Completed; }
    public void Fail() { EnsureNotTerminal(); Status = ProbeStatus.Failed; }
    public void Remove() { EnsureNotTerminal(); Status = ProbeStatus.Removed; }
    public void Expire(DateTime utcNow)
    {
        if (utcNow.Kind != DateTimeKind.Utc) throw new ArgumentException("Time must be UTC.", nameof(utcNow));
        if (utcNow < ExpiresAt) throw new InvalidOperationException("Probe has not reached its expiry time.");
        EnsureNotTerminal(); Status = ProbeStatus.Expired;
    }
    private void EnsureNotTerminal()
    {
        if (Status is ProbeStatus.Completed or ProbeStatus.Failed or ProbeStatus.Expired or ProbeStatus.Removed)
            throw new InvalidOperationException($"Probe in {Status} status cannot transition.");
    }
}
