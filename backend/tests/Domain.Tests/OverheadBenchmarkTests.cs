using System;
using System.Diagnostics;
using RuntimeInvestigation.RuntimeAgent.Instrumentation;
using SimpleApp;
using Xunit;
using Xunit.Abstractions;

namespace Domain.Tests;

public class OverheadBenchmarkTests
{
    private readonly ITestOutputHelper _output;

    public OverheadBenchmarkTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Benchmark_TargetWithAndWithoutProbe_ReportsOverhead()
    {
        const int iterations = 10_000;

        // 1. Baseline: Direct invocation
        var sw = Stopwatch.StartNew();
        long baselineSum = 0;
        for (int i = 0; i < iterations; i++)
        {
            baselineSum += Calculator.Add(i, 1);
        }
        sw.Stop();
        var baselineTicks = sw.ElapsedTicks;
        var baselineMs = sw.Elapsed.TotalMilliseconds;

        // 2. Instrumented invocation via in-process wrapper
        var method = typeof(Calculator).GetMethod(nameof(Calculator.Add))!;
        var wrapper = (Func<int, int, int>)IlWrapperSpike.CreateWrapper(method);

        int loggedCount = 0;
        IlWrapperSpike.OnMethodEntry = (_, _) => loggedCount++;
        IlWrapperSpike.OnMethodExit = null;

        sw.Restart();
        long instrumentedSum = 0;
        for (int i = 0; i < iterations; i++)
        {
            instrumentedSum += wrapper(i, 1);
        }
        sw.Stop();
        var instrumentedTicks = sw.ElapsedTicks;
        var instrumentedMs = sw.Elapsed.TotalMilliseconds;

        IlWrapperSpike.OnMethodEntry = null;

        Assert.Equal(baselineSum, instrumentedSum);
        Assert.Equal(iterations, loggedCount);

        _output.WriteLine($"[Overhead Benchmark] Iterations: {iterations:N0}");
        _output.WriteLine($"  Baseline Duration:     {baselineMs:F3} ms ({baselineTicks:N0} ticks)");
        _output.WriteLine($"  Instrumented Duration: {instrumentedMs:F3} ms ({instrumentedTicks:N0} ticks)");
        _output.WriteLine($"  Environment: {Environment.OSVersion}, .NET {Environment.Version}, {Environment.ProcessorCount} cores");
    }
}
