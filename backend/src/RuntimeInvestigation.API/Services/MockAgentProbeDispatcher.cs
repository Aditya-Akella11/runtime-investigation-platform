using RuntimeInvestigation.Application.Features.Probes;
using RuntimeInvestigation.Shared.Contracts;

namespace RuntimeInvestigation.API.Services;

public sealed class MockAgentProbeDispatcher : IProbeDispatcher, IAgentDispatcher
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

    public Task<AgentRegistrationAck> RegisterAgentAsync(AgentRegistrationCommand command, CancellationToken cancellationToken = default)
    {
        _store.AddAudit("AgentRegistered", command.AgentId, $"Agent '{command.AgentId}' registered with protocol {command.ProtocolVersion}.");
        return Task.FromResult(new AgentRegistrationAck(command.AgentId, AgentProtocol.Version, command.Capabilities, true));
    }

    public Task<ProbeActivationAck> DeployProbeAsync(ProbeActivationCommand command, CancellationToken cancellationToken = default)
    {
        _store.AddAudit("ProbeDispatched", command.ProbeId, $"Probe '{command.ProbeId}' on {command.TargetClass}.{command.TargetMethod} dispatched to agent.");
        return Task.FromResult(new ProbeActivationAck(command.ProbeId, "Active", AgentProtocol.Version, true));
    }

    public Task<ProbeRemovalAck> RemoveProbeAsync(ProbeRemovalCommand command, CancellationToken cancellationToken = default)
    {
        _store.AddAudit("ProbeRemovalDispatched", command.ProbeId, $"Probe '{command.ProbeId}' removal dispatched. Reason: {command.Reason}");
        return Task.FromResult(new ProbeRemovalAck(command.ProbeId, "Removed", AgentProtocol.Version, true));
    }

    public Task<ProbeActivationAck> DispatchActivationAsync(ProbeActivationCommand command, CancellationToken cancellationToken = default) =>
        DeployProbeAsync(command, cancellationToken);

    public Task<ProbeRemovalAck> DispatchRemovalAsync(ProbeRemovalCommand command, CancellationToken cancellationToken = default) =>
        RemoveProbeAsync(command, cancellationToken);
}
