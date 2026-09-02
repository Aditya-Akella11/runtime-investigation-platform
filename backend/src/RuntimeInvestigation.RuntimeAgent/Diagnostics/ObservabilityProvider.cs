using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using RuntimeInvestigation.RuntimeAgent.Safety;

namespace RuntimeInvestigation.RuntimeAgent.Diagnostics;

public sealed record ObservabilityEvent(
    string ProbeId,
    string MethodName,
    IReadOnlyDictionary<string, string> Arguments,
    DateTimeOffset TimestampUtc);

public interface IObservabilitySink
{
    Task ExportAsync(ObservabilityEvent evt, CancellationToken cancellationToken);
}

public sealed class MockObservabilitySink : IObservabilitySink
{
    private readonly ConcurrentBag<ObservabilityEvent> _events = new();

    public IReadOnlyCollection<ObservabilityEvent> ExportedEvents => _events.ToArray();

    public Task ExportAsync(ObservabilityEvent evt, CancellationToken cancellationToken)
    {
        _events.Add(evt);
        return Task.CompletedTask;
    }

    public void Clear() => _events.Clear();
}

public sealed class ObservabilityProvider : IAsyncDisposable
{
    private readonly Channel<ObservabilityEvent> _channel;
    private readonly IObservabilitySink _sink;
    private readonly ProbeRedactionPolicy _redactionPolicy;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _processingTask;

    public bool IsEnabled { get; }
    public int DroppedEvents => _droppedEvents;
    private int _droppedEvents;

    public ObservabilityProvider(IObservabilitySink sink, ProbeRedactionPolicy redactionPolicy, bool isEnabled = false, int maxQueueSize = 2000)
    {
        _sink = sink;
        _redactionPolicy = redactionPolicy;
        IsEnabled = isEnabled;

        var options = new BoundedChannelOptions(maxQueueSize)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true
        };

        _channel = Channel.CreateBounded<ObservabilityEvent>(options);
        _processingTask = Task.Run(ProcessEventsAsync);
    }

    public bool TryRecord(string probeId, string methodName, IReadOnlyDictionary<string, string> arguments)
    {
        if (!IsEnabled)
        {
            return false;
        }

        var redactedArgs = _redactionPolicy.RedactDictionary(arguments);
        var evt = new ObservabilityEvent(probeId, methodName, redactedArgs, DateTimeOffset.UtcNow);

        if (!_channel.Writer.TryWrite(evt))
        {
            Interlocked.Increment(ref _droppedEvents);
            return false;
        }

        return true;
    }

    private async Task ProcessEventsAsync()
    {
        try
        {
            var reader = _channel.Reader;
            while (await reader.WaitToReadAsync(_cts.Token))
            {
                while (reader.TryRead(out var evt))
                {
                    try
                    {
                        await _sink.ExportAsync(evt, _cts.Token);
                    }
                    catch
                    {
                        // Sinks should not crash the provider background loop
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on dispose
        }
    }

    public async ValueTask DisposeAsync()
    {
        _channel.Writer.Complete();
        _cts.Cancel();
        try
        {
            await _processingTask;
        }
        catch
        {
            // Ignore cancel exceptions
        }
        _cts.Dispose();
    }
}
