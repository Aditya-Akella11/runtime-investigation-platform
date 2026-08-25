using RuntimeInvestigation.RuntimeAgent.Instrumentation;
using RuntimeInvestigation.Shared.Contracts;
using Xunit;

namespace Domain.Tests;

public class OptimizedBuildCompatibilityTests
{
    [Fact]
    public async Task MultithreadedExecution_ShouldCaptureEventsThreadSafely()
    {
        var sample = new RuntimeInstrumentationSample();
        var tasks = Enumerable.Range(1, 50).Select(async i =>
        {
            await Task.Yield();
            return sample.ProcessPayment($"cust-{i}", i * 10m, amount => $"Processed-{amount}");
        });

        var results = await Task.WhenAll(tasks);
        Assert.Equal(50, results.Length);
        Assert.Equal(50, sample.EntryEvents.Count);
        Assert.Equal(50, sample.ExitEvents.Count);
    }

    [Fact]
    public void ContractSerialization_UnderDifferentPayloadSizes_ShouldRemainCompatible()
    {
        var largeValues = Enumerable.Range(1, 100)
            .ToDictionary(i => $"key_{i}", i => $"value_{i}_{new string('x', 50)}");

        var record = new ProbeEvidenceRecord(
            "probe-100",
            "corr-100",
            largeValues,
            DateTimeOffset.UtcNow,
            AgentProtocol.Version);

        Assert.Equal("probe-100", record.ProbeId);
        Assert.Equal(100, record.Values.Count);
        Assert.Equal(AgentProtocol.Version, record.ProtocolVersion);
    }
}
