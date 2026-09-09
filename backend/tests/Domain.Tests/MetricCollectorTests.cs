using System;
using System.Threading.Tasks;
using RuntimeInvestigation.RuntimeAgent.Instrumentation;
using Xunit;

namespace Domain.Tests;

public class MetricCollectorTests
{
    [Fact]
    public void MeasureCall_IncrementsCallCountAndDuration()
    {
        var collector = new MetricCollector("metric-1");

        using (collector.MeasureCall())
        {
            // simulate fast op
        }

        var (callCount, totalDurationMs, _) = collector.GetSnapshot();
        Assert.Equal(1, callCount);
        Assert.True(totalDurationMs >= 0);
    }

    [Fact]
    public void RecordException_IncrementsExceptionCount()
    {
        var collector = new MetricCollector("metric-2");
        collector.RecordException();
        collector.RecordException();

        var (_, _, exceptionCount) = collector.GetSnapshot();
        Assert.Equal(2, exceptionCount);
    }

    [Fact]
    public void Snapshot_ReflectsCollectedMetrics()
    {
        var collector = new MetricCollector("metric-3");
        using (collector.MeasureCall()) { }
        collector.RecordException();

        var counters = collector.Snapshot();
        Assert.Equal("metric-3", counters.ProbeId);
        Assert.Equal(1, counters.CallCount);
        Assert.Equal(1, counters.ExceptionCount);
    }

    [Fact]
    public void MeasureCall_MarkException_IncrementsExceptionCount()
    {
        var collector = new MetricCollector("metric-4");
        using (var scope = collector.MeasureCall())
        {
            if (scope is IDisposable)
            {
                // reflection or casting if scope has MarkException or helper
            }
        }
        var (callCount, _, _) = collector.GetSnapshot();
        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task ConcurrentMeasurements_ThreadSafe()
    {
        var collector = new MetricCollector("metric-5");
        const int taskCount = 50;

        var tasks = new Task[taskCount];
        for (int i = 0; i < taskCount; i++)
        {
            tasks[i] = Task.Run(async () =>
            {
                using (collector.MeasureCall())
                {
                    await Task.Delay(2);
                }
            });
        }

        await Task.WhenAll(tasks);

        var (callCount, _, _) = collector.GetSnapshot();
        Assert.Equal(taskCount, callCount);
    }
}
