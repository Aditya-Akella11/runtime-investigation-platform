using System.Collections.Concurrent;

namespace RuntimeInvestigation.API.Services;

public sealed class DemoWorkflowStore
{
    private readonly ConcurrentDictionary<string, InvestigationRecord> _investigations = new();
    private readonly ConcurrentDictionary<string, RuntimeProbeRecord> _probes = new();
    private readonly ConcurrentQueue<EvidenceRecord> _evidence = new();
    private readonly ConcurrentQueue<AuditEntry> _audit = new();

    public IReadOnlyCollection<InvestigationRecord> GetInvestigations() => _investigations.Values.OrderByDescending(x => x.CreatedAt).ToArray();
    public IReadOnlyCollection<RuntimeProbeRecord> GetProbes() => _probes.Values.OrderByDescending(x => x.CreatedAt).ToArray();
    public IReadOnlyCollection<EvidenceRecord> GetEvidence() => _evidence.ToArray().Reverse().ToArray();
    public IReadOnlyCollection<AuditEntry> GetAudit() => _audit.ToArray().Reverse().ToArray();

    public InvestigationRecord CreateInvestigation(string title, string application, string environment, string description)
    {
        var record = new InvestigationRecord(Guid.NewGuid().ToString("N"), title, application, environment, description, "Active", DateTimeOffset.UtcNow);
        _investigations[record.Id] = record;
        AddAudit("InvestigationCreated", record.Id, $"Investigation '{title}' created.");
        return record;
    }

    public RuntimeProbeRecord CreateProbe(CreateProbeRequest request)
    {
        var record = new RuntimeProbeRecord(
            Guid.NewGuid().ToString("N"),
            request.InvestigationId,
            request.Application,
            request.ProbeType,
            request.TargetClass,
            request.TargetMethod,
            request.Condition,
            request.DurationMinutes,
            "Draft",
            DateTimeOffset.UtcNow);

        _probes[record.Id] = record;
        AddAudit("ProbeCreated", record.Id, $"Probe '{record.TargetClass}.{record.TargetMethod}' created.");
        return record;
    }

    public RuntimeProbeRecord? DeployProbe(string id)
    {
        if (!_probes.TryGetValue(id, out var probe))
        {
            return null;
        }

        var deployed = probe with { Status = "Active" };
        _probes[id] = deployed;
        AddAudit("ProbeDeployed", id, $"Probe '{id}' deployed to mock agent.");
        _evidence.Enqueue(new EvidenceRecord(Guid.NewGuid().ToString("N"), id, deployed.Application, deployed.TargetClass, deployed.TargetMethod, "CustomerId=1452; Amount=120; Gateway=Timeout; Retry=1", DateTimeOffset.UtcNow));
        return deployed;
    }

    public RuntimeProbeRecord? RemoveProbe(string id)
    {
        if (!_probes.TryGetValue(id, out var probe))
        {
            return null;
        }

        var removed = probe with { Status = "Removed" };
        _probes[id] = removed;
        AddAudit("ProbeRemoved", id, $"Probe '{id}' removed.");
        return removed;
    }

    public RuntimeProbeRecord? ExpireProbe(string id)
    {
        if (!_probes.TryGetValue(id, out var probe))
        {
            return null;
        }

        var expired = probe with { Status = "Expired" };
        _probes[id] = expired;
        AddAudit("ProbeExpired", id, $"Probe '{id}' expired.");
        return expired;
    }

    public void AddAudit(string eventType, string subjectId, string message) =>
        _audit.Enqueue(new AuditEntry(Guid.NewGuid().ToString("N"), eventType, subjectId, message, DateTimeOffset.UtcNow));
}

public sealed record InvestigationRecord(string Id, string Title, string Application, string Environment, string Description, string Status, DateTimeOffset CreatedAt);
public sealed record RuntimeProbeRecord(string Id, string InvestigationId, string Application, string ProbeType, string TargetClass, string TargetMethod, string Condition, int DurationMinutes, string Status, DateTimeOffset CreatedAt);
public sealed record EvidenceRecord(string Id, string ProbeId, string Application, string TargetClass, string TargetMethod, string Payload, DateTimeOffset CapturedAt);
public sealed record AuditEntry(string Id, string EventType, string SubjectId, string Message, DateTimeOffset CreatedAt);
public sealed record CreateProbeRequest(string InvestigationId, string Application, string ProbeType, string TargetClass, string TargetMethod, string Condition, int DurationMinutes);
