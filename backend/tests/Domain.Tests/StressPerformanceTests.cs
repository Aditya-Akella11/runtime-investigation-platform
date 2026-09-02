using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using RuntimeInvestigation.RuntimeAgent.Instrumentation;
using RuntimeInvestigation.RuntimeAgent.Policy;
using RuntimeInvestigation.RuntimeAgent.Safety;
using RuntimeInvestigation.Shared.Contracts;
using SimpleApp;
using Xunit;
using Xunit.Abstractions;

namespace Domain.Tests;

public class StressPerformanceTests
{
    private readonly ITestOutputHelper _output;

    public StressPerformanceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task StressTest_1000ConcurrentTasks_100000MethodCalls_ExecutesCleanly()
    {
        _ = typeof(SimpleApp.Calculator); // Ensure assembly loaded in AppDomain
        var policy = new LocalProbeSafetyPolicy();
        var redactionPolicy = new ProbeRedactionPolicy();
        var activator = new ProbeActivator(policy, redactionPolicy);

        var command = new ProbeActivationCommand(
            "probe-stress-1",
            "StressTarget",
            "SimpleApp.Calculator",
            "Add",
            DateTimeOffset.UtcNow.AddMinutes(30),
            AgentProtocol.Version);

        var activationResult = activator.Activate(command);
        Assert.True(activationResult.Success);
        Assert.True(activator.TryGetBuffer("probe-stress-1", out var buffer));
        Assert.NotNull(buffer);

        const int taskCount = 1_000;
        const int callsPerTask = 100; // Total 100,000 calls
        var sw = Stopwatch.StartNew();

        long totalSum = 0;
        var tasks = Enumerable.Range(0, taskCount).Select(taskId => Task.Run(() =>
        {
            long localSum = 0;
            for (int i = 0; i < callsPerTask; i++)
            {
                int val = Calculator.Add(taskId, i);
                localSum += val;

                // Non-blocking write to log buffer
                buffer!.TryAppend("probe-stress-1", "SimpleApp.Calculator.Add", new Dictionary<string, string>
                {
                    ["taskId"] = taskId.ToString(),
                    ["iter"] = i.ToString(),
                    ["val"] = val.ToString()
                });
            }
            Interlocked.Add(ref totalSum, localSum);
        })).ToArray();

        await Task.WhenAll(tasks);
        sw.Stop();

        var drained = buffer!.Drain();
        _output.WriteLine($"[Stress Test Completed]");
        _output.WriteLine($"  Tasks: {taskCount:N0}, Total Invocations: {taskCount * callsPerTask:N0}");
        _output.WriteLine($"  Duration: {sw.Elapsed.TotalMilliseconds:F2} ms");
        _output.WriteLine($"  Throughput: {(taskCount * callsPerTask) / sw.Elapsed.TotalSeconds:N0} ops/sec");
        _output.WriteLine($"  Buffer Drained: {drained.Count:N0}, Dropped due to capacity: {buffer.DroppedCount:N0}");

        Assert.True(totalSum > 0);
        Assert.True(buffer.Count == 0); // After drain
    }
}
