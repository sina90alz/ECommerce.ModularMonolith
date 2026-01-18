using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Products.Infrastructure.Messaging;
using Products.Infrastructure.Persistence;
using System.Text.Json;
using Products.Domain.Entities;

namespace Products.Infrastructure.Inbox;

public sealed class OrdersEventsConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMessageBus _bus;

    public OrdersEventsConsumer(IServiceScopeFactory scopeFactory, IMessageBus bus)
    {
        _scopeFactory = scopeFactory;
        _bus = bus;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        // Retry loop so app doesn't die if RabbitMQ is temporarily down
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await _bus.SubscribeAsync(
                    exchange: "ecommerce.events",
                    queue: "products.inbox",
                    routingKey: "orders.#",
                    handler: HandleMessageAsync,
                    ct: ct);

                // subscribe returns immediately; keep the service alive
                await Task.Delay(Timeout.InfiniteTimeSpan, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch
            {
                // simple backoff; upgrade to exponential if you want
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
            }
        }
    }

    private async Task<ConsumeResult> HandleMessageAsync(MessageEnvelope msg, CancellationToken ct)
    {
        // Require MessageId for idempotency
        if (!Guid.TryParse(msg.MessageId, out var messageId))
            return ConsumeResult.DeadLetter("Missing/invalid MessageId");

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ProductsDbContext>();

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        try
        {
            // Insert Inbox row first (DB unique key enforces idempotency)
            db.InboxMessages.Add(new InboxMessage
            {
                Id = messageId,
                Type = msg.RoutingKey,
                ReceivedAtUtc = DateTime.UtcNow
            });

            // Apply side-effects based on routing key
            if (msg.RoutingKey == "orders.order-paid.v1")
            {
                var evt = JsonSerializer.Deserialize<OrderPaidIntegrationEvent>(msg.BodyJson)!;

                var product = await db.Products.FindAsync(new object[] { evt.ProductId }, ct)
                             ?? throw new InvalidOperationException("Product not found");

                product.DecreaseStock(evt.Quantity);
            }
            else if (msg.RoutingKey == "orders.order-cancelled.v1")
            {
                var evt = JsonSerializer.Deserialize<OrderCancelledIntegrationEvent>(msg.BodyJson)!;

                var product = await db.Products.FindAsync(new object[] { evt.ProductId }, ct)
                             ?? throw new InvalidOperationException("Product not found");

                product.IncreaseStock(evt.Quantity);
            }
            else
            {
                // Unknown message type: send to DLQ so you can inspect
                return ConsumeResult.DeadLetter($"Unknown routing key: {msg.RoutingKey}");
            }

            // Mark processed
            var inbox = await db.InboxMessages.FindAsync(new object[] { messageId }, ct);
            inbox!.ProcessedAtUtc = DateTime.UtcNow;

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return ConsumeResult.Ack();
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);

            // If Inbox insert failed due to duplicate PK, treat as already processed => ACK
            // (You can refine this by checking SQL Server error codes.)
            if (ex.Message.Contains("PRIMARY KEY", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
            {
                return ConsumeResult.Ack();
            }

            // Business/format errors: DLQ. Transient infra errors: Retry.
            // For now: DLQ by default so you can inspect.
            return ConsumeResult.DeadLetter(ex.Message);
        }
    }

    // Minimal local contract types (or reference your shared Contracts project)
    private sealed record OrderPaidIntegrationEvent(Guid OrderId, Guid ProductId, decimal ProductPrice, int Quantity);
    private sealed record OrderCancelledIntegrationEvent(Guid OrderId, Guid ProductId, decimal ProductPrice, int Quantity);
}
