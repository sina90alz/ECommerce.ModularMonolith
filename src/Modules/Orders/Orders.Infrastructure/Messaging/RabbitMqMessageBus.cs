using RabbitMQ.Client;
using System.Text;

namespace Orders.Infrastructure.Messaging;

public sealed class RabbitMqMessageBus : IMessageBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqMessageBus(string hostName)
    {
        var factory = new ConnectionFactory
        {
            HostName = hostName,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Direct exchange for integration events
        _channel.ExchangeDeclare(exchange: "ecommerce.events", type: ExchangeType.Topic, durable: true);
    }

    public Task PublishAsync(
        string exchange,
        string routingKey,
        ReadOnlyMemory<byte> body,
        string messageId,
        CancellationToken ct)
    {
        var props = _channel.CreateBasicProperties();
        props.Persistent = true;              // survive broker restart
        props.MessageId = messageId;          // important for Inbox idempotency
        props.ContentType = "application/json";

        _channel.BasicPublish(
            exchange: exchange,
            routingKey: routingKey,
            basicProperties: props,
            body: body);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
