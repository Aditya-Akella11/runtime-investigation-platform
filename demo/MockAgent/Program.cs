using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<MockAgentStore>();
builder.Services.AddSingleton<MockEvidenceGenerator>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "mock-agent", protocol = "v1" }));

app.MapPost("/agents/register", (RegisterAgentRequest request, MockAgentStore store) =>
{
    if (string.IsNullOrWhiteSpace(request.AgentId))
    {
        return Results.BadRequest(new { error = "AgentId is required." });
    }

    store.Register(request.AgentId, request.Capabilities ?? Array.Empty<string>());
    return Results.Ok(new AgentRegistrationResponse(request.AgentId, "v1", request.Capabilities ?? Array.Empty<string>()));
});

app.MapPost("/probes/{probeId}/activate", (string probeId, ActivateProbeRequest request, MockAgentStore store) =>
{
    if (string.IsNullOrWhiteSpace(probeId))
    {
        return Results.BadRequest(new { error = "ProbeId is required." });
    }

    var state = store.Activate(probeId, request.ApplicationName, request.TargetClass, request.TargetMethod, request.ExpiresAtUtc);
    return Results.Ok(state);
});

app.MapPost("/probes/{probeId}/evidence", (string probeId, EvidenceEnvelope envelope, MockAgentStore store, MockEvidenceGenerator generator) =>
{
    if (!store.TryGet(probeId, out var state))
    {
        return Results.NotFound(new { error = "Probe not activated." });
    }

    if (state.Status is not ProbeLifecycle.Activated)
    {
        return Results.Conflict(new { error = $"Probe is {state.Status}." });
    }

    var evidence = generator.Generate(probeId, envelope, state);
    store.AppendEvidence(probeId, evidence);
    return Results.Ok(evidence);
});

app.MapPost("/probes/{probeId}/tick", (string probeId, MockAgentStore store, MockEvidenceGenerator generator) =>
{
    if (!store.TryGet(probeId, out var state))
    {
        return Results.NotFound(new { error = "Probe not found." });
    }

    if (state.Status is ProbeLifecycle.Removed or ProbeLifecycle.Expired)
    {
        return Results.Conflict(new { error = $"Probe is {state.Status}." });
    }

    var evidence = generator.Generate(
        probeId,
        new EvidenceEnvelope(Guid.NewGuid().ToString("N"), new Dictionary<string, string>
        {
            ["CustomerId"] = Random.Shared.Next(1000, 9999).ToString(),
            ["Amount"] = Random.Shared.Next(50, 1000).ToString(),
            ["GatewayResponse"] = Random.Shared.Next(100) < 35 ? "Timeout" : "Success",
            ["Retry"] = Random.Shared.Next(0, 3).ToString()
        }),
        state);

    store.AppendEvidence(probeId, evidence);
    return Results.Ok(evidence);
});

app.MapGet("/probes/{probeId}/results", (string probeId, MockAgentStore store) =>
{
    return store.TryGet(probeId, out var state)
        ? Results.Ok(new { state, results = state.Results })
        : Results.NotFound(new { error = "Probe not found." });
});

app.MapPost("/probes/{probeId}/remove", (string probeId, MockAgentStore store) =>
{
    return store.Remove(probeId)
        ? Results.Ok(new { probeId, status = "removed" })
        : Results.NotFound(new { error = "Probe not found." });
});

app.MapPost("/probes/{probeId}/expire", (string probeId, MockAgentStore store) =>
{
    return store.Expire(probeId)
        ? Results.Ok(new { probeId, status = "expired" })
        : Results.NotFound(new { error = "Probe not found." });
});

app.Run();

public sealed record RegisterAgentRequest(string AgentId, string[]? Capabilities);
public sealed record AgentRegistrationResponse(string AgentId, string ProtocolVersion, IReadOnlyList<string> Capabilities);
public sealed record ActivateProbeRequest(string ApplicationName, string TargetClass, string TargetMethod, DateTimeOffset? ExpiresAtUtc);
public sealed record EvidenceEnvelope(string CorrelationId, IReadOnlyDictionary<string, string> Values);
public sealed record EvidenceRecord(string ProbeId, string CorrelationId, DateTimeOffset CapturedAtUtc, IReadOnlyDictionary<string, string> Values);
public sealed record ProbeState(string ProbeId, string ApplicationName, string TargetClass, string TargetMethod, DateTimeOffset? ExpiresAtUtc, ProbeLifecycle Status, List<EvidenceRecord> Results);

public enum ProbeLifecycle
{
    Activated,
    Removed,
    Expired
}

public sealed class MockAgentStore
{
    private readonly ConcurrentDictionary<string, ProbeState> _probes = new();
    private readonly ConcurrentDictionary<string, (string AgentId, IReadOnlyList<string> Capabilities)> _agents = new();

    public void Register(string agentId, IReadOnlyList<string> capabilities) =>
        _agents[agentId] = (agentId, capabilities);

    public ProbeState Activate(string probeId, string applicationName, string targetClass, string targetMethod, DateTimeOffset? expiresAtUtc)
    {
        var state = new ProbeState(probeId, applicationName, targetClass, targetMethod, expiresAtUtc, ProbeLifecycle.Activated, new List<EvidenceRecord>());
        _probes[probeId] = state;
        return state;
    }

    public bool TryGet(string probeId, out ProbeState state) => _probes.TryGetValue(probeId, out state!);

    public void AppendEvidence(string probeId, EvidenceRecord evidence)
    {
        if (_probes.TryGetValue(probeId, out var state))
        {
            state.Results.Add(evidence);
        }
    }

    public bool Remove(string probeId)
    {
        if (!_probes.TryGetValue(probeId, out var state))
        {
            return false;
        }

        _probes[probeId] = state with { Status = ProbeLifecycle.Removed };
        return true;
    }

    public bool Expire(string probeId)
    {
        if (!_probes.TryGetValue(probeId, out var state))
        {
            return false;
        }

        _probes[probeId] = state with { Status = ProbeLifecycle.Expired };
        return true;
    }
}

public sealed class MockEvidenceGenerator
{
    public EvidenceRecord Generate(string probeId, EvidenceEnvelope envelope, ProbeState state)
    {
        var values = new Dictionary<string, string>(envelope.Values)
        {
            ["application"] = state.ApplicationName,
            ["target"] = $"{state.TargetClass}.{state.TargetMethod}",
            ["probeStatus"] = state.Status.ToString()
        };

        return new EvidenceRecord(probeId, envelope.CorrelationId, DateTimeOffset.UtcNow, values);
    }
}
