namespace Orders.Infrastructure.Messaging;

public interface IMessageBus
{
    Task PublishAsync(
        string exchange,
        string routingKey,
        ReadOnlyMemory<byte> body,
        string messageId,
        CancellationToken ct);
}
