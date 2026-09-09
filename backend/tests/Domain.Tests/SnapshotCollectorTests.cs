using System;
using System.Linq;
using RuntimeInvestigation.RuntimeAgent.Instrumentation;
using Xunit;

namespace Domain.Tests;

public class SnapshotCollectorTests
{
    [Fact]
    public void OnEntry_CapturesArgs()
    {
        var collector = new SnapshotCollector("probe-1");
        collector.OnEntry(new object?[] { "user123", 42 });

        var captures = collector.GetCaptures();
        Assert.Single(captures);
        Assert.Equal("probe-1", captures[0].ProbeId);
        Assert.Equal("user123", captures[0].Variables["args[0]"]);
        Assert.Equal("42", captures[0].Variables["args[1]"]);
    }

    [Fact]
    public void OnEntry_AppliesConditionFilter_MatchesCaptured()
    {
        var collector = new SnapshotCollector("probe-2", "args[0] > 100");
        collector.OnEntry(new object?[] { 150 });

        var captures = collector.GetCaptures();
        Assert.Single(captures);
        Assert.Equal("150", captures[0].Variables["args[0]"]);
    }

    [Fact]
    public void OnEntry_AppliesConditionFilter_NonMatchingIgnored()
    {
        var collector = new SnapshotCollector("probe-3", "args[0] > 100");
        collector.OnEntry(new object?[] { 50 });

        var captures = collector.GetCaptures();
        Assert.Empty(captures);
    }

    [Fact]
    public void OnEntry_HandlesNullArgs()
    {
        var collector = new SnapshotCollector("probe-4");
        collector.OnEntry(new object?[] { null, "valid" });

        var captures = collector.GetCaptures();
        Assert.Single(captures);
        Assert.Null(captures[0].Variables["args[0]"]);
        Assert.Equal("valid", captures[0].Variables["args[1]"]);
    }

    [Fact]
    public void Clear_EmptiesCaptures()
    {
        var collector = new SnapshotCollector("probe-5");
        collector.OnEntry(new object?[] { 1 });
        collector.OnEntry(new object?[] { 2 });
        Assert.Equal(2, collector.GetCaptures().Count);

        collector.Clear();
        Assert.Empty(collector.GetCaptures());
    }
}
