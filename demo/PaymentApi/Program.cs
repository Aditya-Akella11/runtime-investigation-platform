using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<PaymentStore>();
builder.Services.AddSingleton<RandomFailureSimulator>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "demo-payment-api" }));

app.MapPost("/payments", async (CreatePaymentRequest request, PaymentStore store, RandomFailureSimulator simulator, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.OrderId))
    {
        return Results.BadRequest(new { error = "OrderId is required." });
    }

    if (request.Amount <= 0)
    {
        return Results.BadRequest(new { error = "Amount must be greater than zero." });
    }

    await simulator.MaybeSlowDownAsync(cancellationToken);
    simulator.MaybeThrowRandomFailure();

    var existing = store.FindByOrderId(request.OrderId);
    if (existing is not null && simulator.ShouldDuplicateCharge())
    {
        var duplicate = existing with
        {
            Id = Guid.NewGuid(),
            ProcessedAt = DateTimeOffset.UtcNow,
            Status = PaymentStatus.ChargedDuplicate
        };

        store.Upsert(duplicate);
        return Results.Ok(duplicate);
    }

    var payment = new PaymentRecord(
        Guid.NewGuid(),
        request.OrderId,
        request.CustomerId,
        request.Amount,
        PaymentStatus.Charged,
        request.GatewayOverride ?? simulator.NextGatewayResponse(),
        DateTimeOffset.UtcNow);

    store.Upsert(payment);
    return Results.Created($"/payments/{payment.Id}", payment);
});

app.MapGet("/payments/{id:guid}", (Guid id, PaymentStore store) =>
{
    var payment = store.Get(id);
    return payment is null ? Results.NotFound() : Results.Ok(payment);
});

app.MapPost("/refunds", async (CreateRefundRequest request, PaymentStore store, RandomFailureSimulator simulator, CancellationToken cancellationToken) =>
{
    if (request.PaymentId == Guid.Empty)
    {
        return Results.BadRequest(new { error = "PaymentId is required." });
    }

    var payment = store.Get(request.PaymentId);
    if (payment is null)
    {
        return Results.NotFound(new { error = "Payment not found." });
    }

    await simulator.MaybeSlowDownAsync(cancellationToken);
    simulator.MaybeThrowRandomFailure();

    var refunded = payment with
    {
        Status = PaymentStatus.Refunded,
        ProcessedAt = DateTimeOffset.UtcNow
    };

    store.Upsert(refunded);
    return Results.Ok(refunded);
});

app.MapGet("/orders/{id}", (string id, PaymentStore store) =>
{
    var orderPayments = store.GetByOrderId(id);
    if (orderPayments.Count == 0)
    {
        return Results.NotFound(new { error = "Order not found." });
    }

    return Results.Ok(new OrderSummary(id, orderPayments.ToList()));
});

app.Run();

public sealed record CreatePaymentRequest(string OrderId, string CustomerId, decimal Amount, string? GatewayOverride);
public sealed record CreateRefundRequest(Guid PaymentId, string? Reason);
public sealed record OrderSummary(string OrderId, IReadOnlyList<PaymentRecord> Payments);
public sealed record PaymentRecord(Guid Id, string OrderId, string CustomerId, decimal Amount, PaymentStatus Status, string GatewayResponse, DateTimeOffset ProcessedAt);

public enum PaymentStatus
{
    Charged,
    ChargedDuplicate,
    Refunded
}

public sealed class PaymentStore
{
    private readonly ConcurrentDictionary<Guid, PaymentRecord> _payments = new();

    public PaymentRecord? Get(Guid id) => _payments.TryGetValue(id, out var payment) ? payment : null;

    public PaymentRecord? FindByOrderId(string orderId) =>
        _payments.Values.OrderByDescending(payment => payment.ProcessedAt).FirstOrDefault(payment => payment.OrderId == orderId);

    public IReadOnlyCollection<PaymentRecord> GetByOrderId(string orderId) =>
        _payments.Values.Where(payment => payment.OrderId == orderId).OrderByDescending(payment => payment.ProcessedAt).ToArray();

    public void Upsert(PaymentRecord payment) => _payments[payment.Id] = payment;
}

public sealed class RandomFailureSimulator
{
    private readonly Random _random = new();

    public void MaybeThrowRandomFailure()
    {
        var roll = _random.Next(100);

        if (roll < 12)
        {
            throw new InvalidOperationException("Random payment gateway failure.");
        }

        if (roll is >= 12 and < 18)
        {
            throw new NullReferenceException("Intentional null reference failure in demo payment flow.");
        }

        if (roll is >= 18 and < 24)
        {
            throw new TimeoutException("Intentional timeout exception in demo payment flow.");
        }
    }

    public bool ShouldDuplicateCharge() => _random.Next(100) < 20;

    public string NextGatewayResponse()
    {
        var roll = _random.Next(100);
        return roll < 35 ? "Timeout" : roll < 70 ? "RetryAccepted" : "Success";
    }

    public Task MaybeSlowDownAsync(CancellationToken cancellationToken) =>
        Task.Delay(TimeSpan.FromMilliseconds(_random.Next(150, 1200)), cancellationToken);
}
