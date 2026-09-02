using System;
using System.Collections.Generic;
using RuntimeInvestigation.RuntimeAgent.Instrumentation;
using RuntimeInvestigation.RuntimeAgent.Policy;
using RuntimeInvestigation.RuntimeAgent.Safety;
using RuntimeInvestigation.Shared.Contracts;
using Xunit;

namespace Domain.Tests;

public class ProbeActivatorTests
{
    private readonly LocalProbeSafetyPolicy _policy = new();
    private readonly ProbeRedactionPolicy _redactionPolicy = new();

    [Fact]
    public void Activate_ValidMethod_Succeeds()
    {
        var activator = new ProbeActivator(_policy, _redactionPolicy);
        var command = new ProbeActivationCommand(
            "probe-1",
            "DemoApp",
            "System.Console",
            "WriteLine",
            DateTimeOffset.UtcNow.AddMinutes(10),
            AgentProtocol.Version);

        var result = activator.Activate(command);

        Assert.True(result.Success);
        Assert.Equal(ProbeActivationStatus.Active, result.Status);
        Assert.True(activator.IsActive("probe-1"));
    }

    [Fact]
    public void Activate_SecurityNamespace_FailsSafetyPolicy()
    {
        var activator = new ProbeActivator(_policy, _redactionPolicy);
        var command = new ProbeActivationCommand(
            "probe-2",
            "DemoApp",
            "System.Security.Cryptography.Aes",
            "Create",
            DateTimeOffset.UtcNow.AddMinutes(10),
            AgentProtocol.Version);

        var result = activator.Activate(command);

        Assert.False(result.Success);
        Assert.Equal(ProbeActivationStatus.Failed, result.Status);
        Assert.Contains("protected security namespace", result.FailureReason);
        Assert.False(activator.IsActive("probe-2"));
    }

    [Fact]
    public void Activate_UnresolvedMethod_FailsClosed()
    {
        var activator = new ProbeActivator(_policy, _redactionPolicy);
        var command = new ProbeActivationCommand(
            "probe-3",
            "DemoApp",
            "NonExistent.Class",
            "NonExistentMethod",
            DateTimeOffset.UtcNow.AddMinutes(10),
            AgentProtocol.Version);

        var result = activator.Activate(command);

        Assert.False(result.Success);
        Assert.Equal(ProbeActivationStatus.Failed, result.Status);
        Assert.Contains("not found", result.FailureReason);
        Assert.False(activator.IsActive("probe-3"));
    }

    [Fact]
    public void LogBuffer_RedactsAndBoundsEntries()
    {
        var buffer = new LogBuffer(3, _redactionPolicy);
        var args = new Dictionary<string, string>
        {
            ["password"] = "SecretPassword123!",
            ["user"] = "admin"
        };

        bool a1 = buffer.TryAppend("probe-1", "Login", args);
        bool a2 = buffer.TryAppend("probe-1", "Login", args);
        bool a3 = buffer.TryAppend("probe-1", "Login", args);
        bool a4 = buffer.TryAppend("probe-1", "Login", args); // Over capacity

        Assert.True(a1);
        Assert.True(a2);
        Assert.True(a3);
        Assert.False(a4);
        Assert.Equal(1, buffer.DroppedCount);

        var entries = buffer.Drain();
        Assert.Equal(3, entries.Count);
        Assert.Equal("[redacted]", entries[0].Arguments["password"]);
        Assert.Equal("admin", entries[0].Arguments["user"]);
        Assert.Equal(0, buffer.Count);
    }

    [Fact]
    public void Deactivate_FlushesAndRemovesProbe()
    {
        var activator = new ProbeActivator(_policy, _redactionPolicy);
        var command = new ProbeActivationCommand(
            "probe-4",
            "DemoApp",
            "System.Console",
            "WriteLine",
            DateTimeOffset.UtcNow.AddMinutes(10),
            AgentProtocol.Version);

        activator.Activate(command);
        Assert.True(activator.TryGetBuffer("probe-4", out var buffer));
        buffer!.TryAppend("probe-4", "WriteLine", new Dictionary<string, string> { ["msg"] = "Hello" });

        var deactResult = activator.Deactivate("probe-4", "Test deactivation");

        Assert.True(deactResult.Success);
        Assert.Equal(ProbeActivationStatus.Deactivated, deactResult.Status);
        Assert.False(activator.IsActive("probe-4"));
    }

    [Fact]
    public void DeactivateExpired_CleansUpExpiredProbes()
    {
        var activator = new ProbeActivator(_policy, _redactionPolicy);
        // Activate a probe with valid expiration in the future
        var command = new ProbeActivationCommand(
            "probe-5",
            "DemoApp",
            "System.Console",
            "WriteLine",
            DateTimeOffset.UtcNow.AddSeconds(1),
            AgentProtocol.Version);

        activator.Activate(command);
        Assert.True(activator.IsActive("probe-5"));

        // Wait for it to pass expiry
        System.Threading.Thread.Sleep(1100);
        activator.DeactivateExpired();

        Assert.False(activator.IsActive("probe-5"));
    }
}
