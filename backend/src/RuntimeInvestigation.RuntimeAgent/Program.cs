using RuntimeInvestigation.RuntimeAgent.Diagnostics;
using RuntimeInvestigation.RuntimeAgent.Instrumentation;
using RuntimeInvestigation.RuntimeAgent.Policy;
using RuntimeInvestigation.RuntimeAgent.Safety;
using RuntimeInvestigation.Shared.Contracts;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<AgentCapabilityCatalog>();
builder.Services.AddSingleton<LocalProbeSafetyPolicy>();
builder.Services.AddSingleton<ProbeRedactionPolicy>();
builder.Services.AddSingleton<RuntimeInstrumentationSample>();

var app = builder.Build();

app.MapGet("/health", (AgentCapabilityCatalog catalog) =>
    Results.Ok(new { status = "healthy", service = "runtime-agent", protocol = AgentProtocol.Version, capabilities = catalog.GetCapabilities() }));

app.MapPost("/register", (AgentRegistrationCommand command, AgentCapabilityCatalog catalog) =>
{
    var approved = catalog.GetCapabilities().Intersect(command.Capabilities).ToArray();
    return Results.Ok(new { command.AgentId, command.ProtocolVersion, approvedCapabilities = approved });
});

app.MapGet("/diagnostics", (RuntimeAgentDiagnostics diagnostics) => Results.Ok(diagnostics.Snapshot()));

app.MapPost("/simulate", (RuntimeInstrumentationSample sample, ProbeRedactionPolicy redactionPolicy) =>
{
    var output = sample.ProcessPayment(
        redactionPolicy.Redact("CustomerId", "1452"),
        120m,
        amount => amount > 100m ? "Timeout" : "Success");

    return Results.Ok(new { output });
});

app.Run();
