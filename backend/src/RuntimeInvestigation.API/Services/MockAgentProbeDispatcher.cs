using RuntimeInvestigation.Shared.Contracts;

namespace RuntimeInvestigation.API.Services;

public sealed class MockAgentProbeDispatcher : IProbeDispatcher
{
    private readonly DemoWorkflowStore _store;

    public MockAgentProbeDispatcher(DemoWorkflowStore store)
    {
        _store = store;
    }

    public Task<AgentRegistrationCommand> RegisterAsync(string agentId, IReadOnlyList<string> capabilities, CancellationToken cancellationToken = default)
    {
        _store.AddAudit("AgentRegistered", agentId, $"Agent '{agentId}' registered with the mock dispatcher.");
        return Task.FromResult(new AgentRegistrationCommand(agentId, capabilities, AgentProtocol.Version));
    }

    public Task<ProbeActivationCommand> DeployAsync(string probeId, string application, string targetClass, string targetMethod, DateTimeOffset expiresAtUtc, CancellationToken cancellationToken = default)
    {
        _store.AddAudit("ProbeDispatched", probeId, $"Probe '{probeId}' dispatched to the mock agent.");
        return Task.FromResult(new ProbeActivationCommand(probeId, application, targetClass, targetMethod, expiresAtUtc, AgentProtocol.Version));
    }

    public Task<ProbeRemovalCommand> RemoveAsync(string probeId, string reason, CancellationToken cancellationToken = default)
    {
        _store.AddAudit("ProbeRemovalDispatched", probeId, $"Probe '{probeId}' removal dispatched. Reason: {reason}");
        return Task.FromResult(new ProbeRemovalCommand(probeId, reason, AgentProtocol.Version));
    }
}
