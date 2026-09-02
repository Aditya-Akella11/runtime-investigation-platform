using System;
using System.Collections.Generic;
using System.Reflection;
using RuntimeInvestigation.RuntimeAgent.Instrumentation;
using RuntimeInvestigation.RuntimeAgent.Policy;
using RuntimeInvestigation.RuntimeAgent.Safety;
using RuntimeInvestigation.Shared.Contracts;
using SimpleApp;
using Xunit;

namespace Domain.Tests;

public class SimpleAppIntegrationTests
{
    private readonly LocalProbeSafetyPolicy _policy = new();
    private readonly ProbeRedactionPolicy _redactionPolicy = new();

    [Fact]
    public void SimpleApp_Methods_CanBeResolvedAndInstrumented()
    {
        _ = typeof(SimpleApp.Calculator); // Ensure assembly loaded in AppDomain
        // 1. Resolve SimpleApp.Calculator.Add
        var method = ProbeMethodFinder.FindMethod("SimpleApp.Calculator.Add");
        Assert.NotNull(method);
        Assert.Equal("Add", method!.Name);

        // 2. Wrap using in-process IlWrapperSpike
        var capturedEntries = new List<(string method, object?[] args)>();
        IlWrapperSpike.OnMethodEntry = (m, a) => capturedEntries.Add((m, a));

        var wrapper = IlWrapperSpike.CreateWrapper(method);

        // 3. Execute 5 calls and collect evidence
        for (int i = 1; i <= 5; i++)
        {
            var res = wrapper.DynamicInvoke(i, 10);
            Assert.Equal(i + 10, res);
        }

        Assert.Equal(5, capturedEntries.Count);
        Assert.All(capturedEntries, e => Assert.Contains("Calculator.Add", e.method));

        IlWrapperSpike.OnMethodEntry = null;
    }

    [Fact]
    public void SimpleApp_Activation_WithProbeActivator_CollectsEvidence()
    {
        var activator = new ProbeActivator(_policy, _redactionPolicy);
        var command = new ProbeActivationCommand(
            "probe-simpleapp-1",
            "SimpleApp",
            "SimpleApp.Calculator",
            "Add",
            DateTimeOffset.UtcNow.AddMinutes(15),
            AgentProtocol.Version);

        var result = activator.Activate(command);

        Assert.True(result.Success);
        Assert.Equal(ProbeActivationStatus.Active, result.Status);

        Assert.True(activator.TryGetBuffer("probe-simpleapp-1", out var buffer));
        Assert.NotNull(buffer);

        // Simulate 5 method invocations into the buffer
        for (int i = 0; i < 5; i++)
        {
            buffer!.TryAppend("probe-simpleapp-1", "SimpleApp.Calculator.Add", new Dictionary<string, string>
            {
                ["a"] = i.ToString(),
                ["b"] = "10"
            }, (i + 10).ToString());
        }

        var drained = buffer!.Drain();
        Assert.Equal(5, drained.Count);
        Assert.Equal("14", drained[4].ReturnValue);
    }
}
