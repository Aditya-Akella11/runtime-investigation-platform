namespace RuntimeInvestigation.RuntimeAgent.Instrumentation;

public sealed class RuntimeInstrumentationSample
{
    public string ProcessPayment(string customerId, decimal amount, Func<decimal, string> gatewayCall)
    {
        var target = $"{nameof(RuntimeInstrumentationSample)}.{nameof(ProcessPayment)}";
        var started = DateTimeOffset.UtcNow;
        var gatewayResponse = gatewayCall(amount);
        var duration = DateTimeOffset.UtcNow - started;

        return $"{target} customer={customerId} amount={amount} gateway={gatewayResponse} durationMs={duration.TotalMilliseconds:F0}";
    }
}
