using System.Collections.Concurrent;

namespace RuntimeInvestigation.RuntimeAgent.Instrumentation;

public sealed record MethodEntryEvent(string MethodName, IReadOnlyDictionary<string, string> Parameters, DateTimeOffset TimestampUtc);
public sealed record MethodExitEvent(string MethodName, string ReturnValue, TimeSpan Duration, DateTimeOffset TimestampUtc);
public sealed record ExceptionEvent(string MethodName, string ExceptionType, string Message, DateTimeOffset TimestampUtc);

public sealed class RuntimeInstrumentationSample
{
    private readonly ConcurrentQueue<MethodEntryEvent> _entryEvents = new();
    private readonly ConcurrentQueue<MethodExitEvent> _exitEvents = new();
    private readonly ConcurrentQueue<ExceptionEvent> _exceptionEvents = new();

    public IReadOnlyList<MethodEntryEvent> EntryEvents => _entryEvents.ToArray();
    public IReadOnlyList<MethodExitEvent> ExitEvents => _exitEvents.ToArray();
    public IReadOnlyList<ExceptionEvent> ExceptionEvents => _exceptionEvents.ToArray();

    public string ProcessPayment(string customerId, decimal amount, Func<decimal, string> gatewayCall)
    {
        var target = $"{nameof(RuntimeInstrumentationSample)}.{nameof(ProcessPayment)}";
        var started = DateTimeOffset.UtcNow;

        _entryEvents.Enqueue(new MethodEntryEvent(target, new Dictionary<string, string>
        {
            ["customerId"] = customerId,
            ["amount"] = amount.ToString("F2")
        }, started));

        try
        {
            var gatewayResponse = gatewayCall(amount);
            var duration = DateTimeOffset.UtcNow - started;

            _exitEvents.Enqueue(new MethodExitEvent(target, gatewayResponse, duration, DateTimeOffset.UtcNow));

            return $"{target} customer={customerId} amount={amount} gateway={gatewayResponse} durationMs={duration.TotalMilliseconds:F0}";
        }
        catch (Exception ex)
        {
            _exceptionEvents.Enqueue(new ExceptionEvent(target, ex.GetType().Name, ex.Message, DateTimeOffset.UtcNow));
            throw;
        }
    }

    public async Task<string> ProcessPaymentAsync(string customerId, decimal amount, Func<decimal, Task<string>> gatewayCallAsync)
    {
        var target = $"{nameof(RuntimeInstrumentationSample)}.{nameof(ProcessPaymentAsync)}";
        var started = DateTimeOffset.UtcNow;

        _entryEvents.Enqueue(new MethodEntryEvent(target, new Dictionary<string, string>
        {
            ["customerId"] = customerId,
            ["amount"] = amount.ToString("F2")
        }, started));

        try
        {
            var gatewayResponse = await gatewayCallAsync(amount);
            var duration = DateTimeOffset.UtcNow - started;

            _exitEvents.Enqueue(new MethodExitEvent(target, gatewayResponse, duration, DateTimeOffset.UtcNow));

            return $"{target} customer={customerId} amount={amount} gateway={gatewayResponse} durationMs={duration.TotalMilliseconds:F0}";
        }
        catch (Exception ex)
        {
            _exceptionEvents.Enqueue(new ExceptionEvent(target, ex.GetType().Name, ex.Message, DateTimeOffset.UtcNow));
            throw;
        }
    }

    public void Clear()
    {
        while (_entryEvents.TryDequeue(out _)) { }
        while (_exitEvents.TryDequeue(out _)) { }
        while (_exceptionEvents.TryDequeue(out _)) { }
    }
}
