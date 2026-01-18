using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Products.Infrastructure.Messaging;

public sealed class RabbitMqMessageBus : IMessageBus, IDisposable
{
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqMessageBus()
    {
        _factory = new ConnectionFactory
        {
            HostName = "localhost",
            DispatchConsumersAsync = true
        };
    }

    public Task SubscribeAsync(
        string exchange,
        string queue,
        string routingKey,
        Func<MessageEnvelope, CancellationToken, Task<ConsumeResult>> handler,
        CancellationToken ct)
    {
        EnsureConnected(exchange, queue, routingKey);

        var consumer = new AsyncEventingBasicConsumer(_channel!);

        consumer.Received += async (_, ea) =>
        {
            if (ct.IsCancellationRequested)
                return;

            var messageId = ea.BasicProperties?.MessageId;
            var bodyJson = Encoding.UTF8.GetString(ea.Body.ToArray());
            var envelope = new MessageEnvelope(ea.RoutingKey, bodyJson, messageId);

            ConsumeResult result;
            try
            {
                result = await handler(envelope, ct);
            }
            catch (Exception ex)
            {
                // If handler throws, treat as transient by default
                result = ConsumeResult.Retry(ex.Message);
            }

            // Apply ack policy
            switch (result.Disposition)
            {
                case ConsumeDisposition.Ack:
                    _channel!.BasicAck(ea.DeliveryTag, false);
                    break;

                case ConsumeDisposition.Retry:
                    _channel!.BasicNack(ea.DeliveryTag, false, requeue: true);
                    break;

                case ConsumeDisposition.DeadLetter:
                    _channel!.BasicReject(ea.DeliveryTag, requeue: false);
                    break;
            }
        };

        _channel!.BasicConsume(queue: queue, autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }

    private void EnsureConnected(string exchange, string queue, string routingKey)
    {
        if (_connection is not null && _channel is not null && _connection.IsOpen)
            return;

        _connection = _factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Exchange (topic)
        _channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Topic, durable: true);
        // Queue
        _channel.QueueDeclare(queue: queue, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(queue: queue, exchange: exchange, routingKey: routingKey);

        // Prefetch=1 for correctness-first
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
    }

    public void Dispose()
    {
        try { _channel?.Close(); } catch { }
        try { _connection?.Close(); } catch { }
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
