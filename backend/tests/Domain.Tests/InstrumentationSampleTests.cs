using RuntimeInvestigation.RuntimeAgent.Instrumentation;
using Xunit;

namespace Domain.Tests;

public class InstrumentationSampleTests
{
    [Fact]
    public void ProcessPayment_ShouldCaptureEntryAndExitEvents()
    {
        var sample = new RuntimeInstrumentationSample();
        var result = sample.ProcessPayment("cust-100", 250m, amount => "Success");

        Assert.Contains("ProcessPayment", result);
        Assert.Single(sample.EntryEvents);
        Assert.Single(sample.ExitEvents);
        Assert.Empty(sample.ExceptionEvents);

        var entry = sample.EntryEvents[0];
        Assert.Equal("cust-100", entry.Parameters["customerId"]);
        Assert.Equal("250.00", entry.Parameters["amount"]);

        var exit = sample.ExitEvents[0];
        Assert.Equal("Success", exit.ReturnValue);
    }

    [Fact]
    public void ProcessPayment_WhenGatewayThrows_ShouldCaptureExceptionEvent()
    {
        var sample = new RuntimeInstrumentationSample();

        Assert.Throws<TimeoutException>(() =>
            sample.ProcessPayment("cust-100", 250m, _ => throw new TimeoutException("Gateway timeout")));

        Assert.Single(sample.EntryEvents);
        Assert.Empty(sample.ExitEvents);
        Assert.Single(sample.ExceptionEvents);

        var exEvent = sample.ExceptionEvents[0];
        Assert.Equal("TimeoutException", exEvent.ExceptionType);
        Assert.Equal("Gateway timeout", exEvent.Message);
    }

    [Fact]
    public async Task ProcessPaymentAsync_ShouldCaptureAsyncExecution()
    {
        var sample = new RuntimeInstrumentationSample();
        var result = await sample.ProcessPaymentAsync("cust-200", 75m, async amount =>
        {
            await Task.Delay(10);
            return "Approved";
        });

        Assert.Contains("Approved", result);
        Assert.Single(sample.EntryEvents);
        Assert.Single(sample.ExitEvents);
        Assert.True(sample.ExitEvents[0].Duration.TotalMilliseconds >= 5);
    }
}
