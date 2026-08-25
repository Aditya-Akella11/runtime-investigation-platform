using RuntimeInvestigation.Domain.Entities;
using RuntimeInvestigation.Shared.Contracts;
using Xunit;

namespace Domain.Tests;

public class ExpiryAndRemovalGuaranteeTests
{
    [Fact]
    public void ExpiredProbe_CannotBeReactivated()
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(5);
        var probe = new RuntimeProbe("inv-1", ProbeType.Log, "PaymentService.ProcessPayment", null, expiresAt);

        // Advance time past expiry
        var future = expiresAt.AddSeconds(1);
        probe.Expire(future);

        Assert.Equal(ProbeStatus.Expired, probe.Status);
        Assert.Throws<InvalidOperationException>(() => probe.Activate());
    }

    [Fact]
    public void RemovedProbe_CannotBeReactivatedOrTransitioned()
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(15);
        var probe = new RuntimeProbe("inv-1", ProbeType.Log, "PaymentService.ProcessPayment", null, expiresAt);

        probe.Activate();
        Assert.Equal(ProbeStatus.Active, probe.Status);

        probe.Remove();
        Assert.Equal(ProbeStatus.Removed, probe.Status);

        Assert.Throws<InvalidOperationException>(() => probe.Activate());
        Assert.Throws<InvalidOperationException>(() => probe.Complete());
    }

    [Fact]
    public void ProbeCreation_RequiresFutureExpiry()
    {
        var past = DateTime.UtcNow.AddMinutes(-5);
        Assert.Throws<ArgumentException>(() =>
            new RuntimeProbe("inv-1", ProbeType.Log, "PaymentService.ProcessPayment", null, past));
    }
}
