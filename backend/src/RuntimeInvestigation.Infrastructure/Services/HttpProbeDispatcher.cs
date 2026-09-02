using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using RuntimeInvestigation.Application.Features.Probes;
using RuntimeInvestigation.Shared.Contracts;

namespace RuntimeInvestigation.Infrastructure.Services;

public sealed class HttpProbeDispatcher : IAgentDispatcher
{
    private readonly HttpClient _httpClient;

    public HttpProbeDispatcher(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    public async Task<ProbeActivationAck> DispatchActivationAsync(ProbeActivationCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/probes/activate", command, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var ack = await response.Content.ReadFromJsonAsync<ProbeActivationAck>(cancellationToken: cancellationToken);
                return ack ?? new ProbeActivationAck(command.ProbeId, "Active", AgentProtocol.Version, true);
            }

            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return new ProbeActivationAck(command.ProbeId, "Failed", AgentProtocol.Version, false, $"Agent returned {(int)response.StatusCode}: {errorBody}");
        }
        catch (Exception ex)
        {
            return new ProbeActivationAck(command.ProbeId, "Failed", AgentProtocol.Version, false, $"Dispatch network error: {ex.Message}");
        }
    }

    public async Task<ProbeRemovalAck> DispatchRemovalAsync(ProbeRemovalCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/probes/{command.ProbeId}/remove", command, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var ack = await response.Content.ReadFromJsonAsync<ProbeRemovalAck>(cancellationToken: cancellationToken);
                return ack ?? new ProbeRemovalAck(command.ProbeId, "Removed", AgentProtocol.Version, true);
            }

            return new ProbeRemovalAck(command.ProbeId, "Failed", AgentProtocol.Version, false);
        }
        catch
        {
            return new ProbeRemovalAck(command.ProbeId, "Failed", AgentProtocol.Version, false);
        }
    }
}
