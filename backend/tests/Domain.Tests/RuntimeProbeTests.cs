using RuntimeInvestigation.Domain.Entities;
namespace Domain.Tests;
public class RuntimeProbeTests
{
    [Fact] public void Constructor_RequiresFutureExpiry() => Assert.Throws<ArgumentException>(() => new RuntimeProbe("investigation", ProbeType.Log, "Service.Method", null, DateTime.UtcNow.AddSeconds(-1)));
    [Fact]
    public void Probe_CanActivateAndComplete()
    {
        var probe = new RuntimeProbe("investigation", ProbeType.Snapshot, "Service.Method", "userId == 1", DateTime.UtcNow.AddMinutes(5));
        probe.Activate(); probe.Complete();
        Assert.Equal(ProbeStatus.Completed, probe.Status);
        Assert.Throws<InvalidOperationException>(() => probe.Remove());
    }
    [Fact]
    public void Probe_CannotExpireBeforeDeadline()
    {
        var probe = new RuntimeProbe("investigation", ProbeType.Metric, "Service.Method", null, DateTime.UtcNow.AddMinutes(5));
        Assert.Throws<InvalidOperationException>(() => probe.Expire(DateTime.UtcNow));
    }
}
