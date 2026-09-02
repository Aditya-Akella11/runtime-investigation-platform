using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RuntimeInvestigation.RuntimeAgent.Diagnostics;
using RuntimeInvestigation.RuntimeAgent.Safety;
using Xunit;

namespace Domain.Tests;

public class ObservabilityProviderTests
{
    private readonly ProbeRedactionPolicy _redactionPolicy = new();

    [Fact]
    public async Task ObservabilityProvider_WhenEnabled_ExportsEventsAsynchronously()
    {
        var sink = new MockObservabilitySink();
        await using var provider = new ObservabilityProvider(sink, _redactionPolicy, isEnabled: true);

        var args = new Dictionary<string, string>
        {
            ["OrderId"] = "1001",
            ["Secret"] = "SensitiveToken123"
        };

        bool recorded = provider.TryRecord("probe-obs-1", "ProcessPayment", args);

        Assert.True(recorded);

        // Wait briefly for the bounded background processing
        for (int i = 0; i < 20; i++)
        {
            if (sink.ExportedEvents.Count > 0) break;
            await Task.Delay(50);
        }

        Assert.NotEmpty(sink.ExportedEvents);
        var exported = Assert.Single(sink.ExportedEvents);
        Assert.Equal("probe-obs-1", exported.ProbeId);
        Assert.Equal("1001", exported.Arguments["OrderId"]);
        Assert.Equal("[redacted]", exported.Arguments["Secret"]);
    }

    [Fact]
    public async Task ObservabilityProvider_WhenDisabled_DoesNotRecord()
    {
        var sink = new MockObservabilitySink();
        await using var provider = new ObservabilityProvider(sink, _redactionPolicy, isEnabled: false);

        bool recorded = provider.TryRecord("probe-obs-2", "ProcessPayment", new Dictionary<string, string>());

        Assert.False(recorded);
        Assert.Empty(sink.ExportedEvents);
    }
}
