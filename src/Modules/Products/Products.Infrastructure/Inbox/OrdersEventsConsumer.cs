using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Products.Infrastructure.Persistence;

namespace Products.Infrastructure.Inbox;

public sealed class OrdersEventsConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private IConnection? _connection;
    private IModel? _channel;

    public OrdersEventsConsumer(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public override Task StartAsync(CancellationToken ct)
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare("ecommerce.events", ExchangeType.Topic, durable: true);
        _channel.QueueDeclare("products.inbox", durable: true, exclusive: false, autoDelete: false);

        // Subscribe to Orders events
        _channel.QueueBind("products.inbox", "ecommerce.events", routingKey: "orders.*");

        // Prefetch = 1 keeps it simple/serial
        _channel.BasicQos(0, 1, false);

        return base.StartAsync(ct);
    }

    protected override Task ExecuteAsync(CancellationToken ct)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel!);

        consumer.Received += async (_, ea) =>
        {
            var messageId = ea.BasicProperties?.MessageId;

            // If publisher always sets MessageId, enforce it.
            if (!Guid.TryParse(messageId, out var id))
            {
                // reject poison (or dead-letter queue later)
                _channel!.BasicAck(ea.DeliveryTag, multiple: false);
                return;
            }

            var type = ea.RoutingKey;

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();

            // Inbox idempotency: if already processed, ACK and exit
            var exists = await db.InboxMessages.FindAsync(new object[] { id }, ct);
            if (exists is not null)
            {
                _channel!.BasicAck(ea.DeliveryTag, multiple: false);
                return;
            }

            // Insert Inbox row first (same transaction as any side effects you do)
            db.InboxMessages.Add(new InboxMessage
            {
                Id = id,
                Type = type,
                ReceivedAtUtc = DateTime.UtcNow
            });

            await db.SaveChangesAsync(ct);

            // Mark processed (separate save, or combine with real side-effects)
            var inbox = await db.InboxMessages.FindAsync(new object[] { id }, ct);
            inbox!.ProcessedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);

            _channel!.BasicAck(ea.DeliveryTag, multiple: false);
        };

        _channel!.BasicConsume(queue: "products.inbox", autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
