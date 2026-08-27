using RuntimeInvestigation.RuntimeAgent.Diagnostics;
using RuntimeInvestigation.RuntimeAgent.Instrumentation;
using RuntimeInvestigation.RuntimeAgent.Policy;
using RuntimeInvestigation.RuntimeAgent.Safety;
using RuntimeInvestigation.Shared.Contracts;

// Parse command line arguments
string? targetPid = null;
for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--pid" && i + 1 < args.Length)
    {
        targetPid = args[i + 1];
        break;
    }
}

if (!string.IsNullOrEmpty(targetPid))
{
    Console.WriteLine($"Agent started, PID: {targetPid}");
}

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<AgentCapabilityCatalog>();
builder.Services.AddSingleton<RuntimeAgentDiagnostics>();
builder.Services.AddSingleton<LocalProbeSafetyPolicy>();
builder.Services.AddSingleton<ProbeRedactionPolicy>();
builder.Services.AddSingleton<RuntimeInstrumentationSample>();

var app = builder.Build();

app.MapGet("/health", (AgentCapabilityCatalog catalog) =>
    Results.Ok(new
    {
        status = "healthy",
        service = "runtime-agent",
        protocol = AgentProtocol.Version,
        capabilities = catalog.GetCapabilities()
    }));

app.MapPost("/register", (AgentRegistrationCommand command, AgentCapabilityCatalog catalog) =>
{
    if (string.IsNullOrWhiteSpace(command.AgentId))
    {
        return Results.BadRequest(new AgentRegistrationAck(command.AgentId ?? string.Empty, AgentProtocol.Version, Array.Empty<string>(), false, "AgentId is required."));
    }

    var approved = catalog.GetCapabilities().Intersect(command.Capabilities, StringComparer.OrdinalIgnoreCase).ToArray();
    return Results.Ok(new AgentRegistrationAck(command.AgentId, AgentProtocol.Version, approved, true));
});

app.MapPost("/probes/activate", (ProbeActivationCommand command, LocalProbeSafetyPolicy safetyPolicy) =>
{
    var (isAllowed, reason) = safetyPolicy.ValidateActivation(command);
    if (!isAllowed)
    {
        return Results.BadRequest(new ProbeActivationAck(command.ProbeId, "Rejected", AgentProtocol.Version, false, reason));
    }

    return Results.Ok(new ProbeActivationAck(command.ProbeId, "Active", AgentProtocol.Version, true));
});

app.MapPost("/probes/{probeId}/remove", (string probeId, ProbeRemovalCommand command) =>
{
    return Results.Ok(new ProbeRemovalAck(probeId, "Removed", AgentProtocol.Version, true));
});

app.MapGet("/diagnostics", (RuntimeAgentDiagnostics diagnostics) => Results.Ok(diagnostics.Snapshot()));
app.MapGet("/metrics", (RuntimeAgentDiagnostics diagnostics) => Results.Ok(diagnostics.Snapshot()));

app.MapPost("/simulate", (RuntimeInstrumentationSample sample, ProbeRedactionPolicy redactionPolicy) =>
{
    var output = sample.ProcessPayment(
        redactionPolicy.Redact("CustomerId", "1452"),
        120m,
        amount => amount > 100m ? "Timeout" : "Success");

    return Results.Ok(new { output });
});

app.Run();

public partial class RuntimeAgentProgram { }
