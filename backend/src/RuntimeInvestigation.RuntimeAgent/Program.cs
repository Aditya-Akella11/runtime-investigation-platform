using RuntimeInvestigation.RuntimeAgent.Diagnostics;
using RuntimeInvestigation.RuntimeAgent.Policy;
using RuntimeInvestigation.Shared.Contracts;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<AgentCapabilityCatalog>();
builder.Services.AddSingleton<LocalProbeSafetyPolicy>();

var app = builder.Build();

app.MapGet("/health", (AgentCapabilityCatalog catalog) =>
    Results.Ok(new { status = "healthy", service = "runtime-agent", protocol = AgentProtocol.Version, capabilities = catalog.GetCapabilities() }));

app.MapPost("/register", (AgentRegistrationCommand command, AgentCapabilityCatalog catalog) =>
{
    var approved = catalog.GetCapabilities().Intersect(command.Capabilities).ToArray();
    return Results.Ok(new { command.AgentId, command.ProtocolVersion, approvedCapabilities = approved });
});

app.MapGet("/diagnostics", (RuntimeAgentDiagnostics diagnostics) => Results.Ok(diagnostics.Snapshot()));

app.Run();
