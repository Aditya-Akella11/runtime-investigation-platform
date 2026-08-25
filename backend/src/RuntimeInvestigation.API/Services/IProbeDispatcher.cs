using RuntimeInvestigation.Shared.Contracts;

namespace RuntimeInvestigation.API.Services;

public interface IProbeDispatcher
{
    Task<AgentRegistrationCommand> RegisterAsync(string agentId, IReadOnlyList<string> capabilities, CancellationToken cancellationToken = default);
    Task<ProbeActivationCommand> DeployAsync(string probeId, string application, string targetClass, string targetMethod, DateTimeOffset expiresAtUtc, CancellationToken cancellationToken = default);
    Task<ProbeRemovalCommand> RemoveAsync(string probeId, string reason, CancellationToken cancellationToken = default);

    Task<AgentRegistrationAck> RegisterAgentAsync(AgentRegistrationCommand command, CancellationToken cancellationToken = default);
    Task<ProbeActivationAck> DeployProbeAsync(ProbeActivationCommand command, CancellationToken cancellationToken = default);
    Task<ProbeRemovalAck> RemoveProbeAsync(ProbeRemovalCommand command, CancellationToken cancellationToken = default);
}
