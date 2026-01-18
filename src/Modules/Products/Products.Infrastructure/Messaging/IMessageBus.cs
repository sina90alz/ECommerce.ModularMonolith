namespace Products.Infrastructure.Messaging;

public interface IMessageBus
{
    Task SubscribeAsync(
        string exchange,
        string queue,
        string routingKey,
        Func<MessageEnvelope, CancellationToken, Task<ConsumeResult>> handler,
        CancellationToken ct);
}

public sealed record MessageEnvelope(
    string RoutingKey,
    string BodyJson,
    string? MessageId);

public sealed record ConsumeResult(ConsumeDisposition Disposition, string? Error = null)
{
    public static ConsumeResult Ack() => new(ConsumeDisposition.Ack);
    public static ConsumeResult Retry(string? error = null) => new(ConsumeDisposition.Retry, error);
    public static ConsumeResult DeadLetter(string? error = null) => new(ConsumeDisposition.DeadLetter, error);
}

public enum ConsumeDisposition
{
    Ack,        // success
    Retry,      // transient failure -> requeue
    DeadLetter  // poison -> reject / DLQ
}
