using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Orders.Infrastructure.Persistence;
using Orders.Infrastructure.Messaging;
using System.Text;

namespace Orders.Infrastructure.Outbox;

public sealed class OutboxPublisher : BackgroundService
{
    private const int BatchSize = 20;
    private const int MaxAttempts = 10;

    private readonly IServiceScopeFactory _scopeFactory;

    public OutboxPublisher(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

                var now = DateTime.UtcNow;

                var messages = await db.OutboxMessages
                    .Where(m =>
                        m.ProcessedOnUtc == null &&
                        m.DeadLetteredOnUtc == null &&
                        (m.NextAttemptOnUtc == null || m.NextAttemptOnUtc <= now))
                    .OrderBy(m => m.OccurredOnUtc)
                    .Take(BatchSize)
                    .ToListAsync(ct);

                if (messages.Count == 0)
                {
                    await Task.Delay(2000, ct);
                    continue;
                }

                foreach (var msg in messages)
                {
                    try
                    {
                        // Routing key should be stable, semantic, and versioned
                        // Example: orders.order-paid.v1
                        var routingKey = RoutingKeyFromType(msg.Type);
                        var bus = scope.ServiceProvider.GetRequiredService<IMessageBus>();

                        msg.ProcessedOnUtc = DateTime.UtcNow;
                        msg.LastError = null;
                        msg.NextAttemptOnUtc = null;
                        var body = Encoding.UTF8.GetBytes(msg.Payload);

                        await bus.PublishAsync(
                            exchange: "ecommerce.events",
                            routingKey: routingKey,
                            body: body,
                            messageId: msg.Id.ToString(),
                            ct: ct);
                    }
                    catch (Exception ex)
                    {
                        msg.AttemptCount++;
                        msg.LastError = ex.Message;

                        if (msg.AttemptCount >= MaxAttempts)
                        {
                            msg.DeadLetteredOnUtc = DateTime.UtcNow;
                        }
                        else
                        {
                            msg.NextAttemptOnUtc = ComputeNextAttemptUtc(msg.AttemptCount);
                        }
                    }
                }

                await db.SaveChangesAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OutboxPublisher error: {ex.Message}");
                await Task.Delay(5000, ct);
            }
        }
    }

    private static DateTime ComputeNextAttemptUtc(int attemptCount)
    {
        var delaySeconds = Math.Min(Math.Pow(2, attemptCount), 300);
        return DateTime.UtcNow.AddSeconds(delaySeconds);
    }

    private static string RoutingKeyFromType(string type)
    {
        // Keep this explicit and stable (treat as API contract).
        // If you already store RoutingKey in OutboxMessage, then use it instead.
        return type switch
        {
            "OrderPaidIntegrationEvent" => "orders.order-paid.v1",
            "OrderCancelledIntegrationEvent" => "orders.order-cancelled.v1",
            _ => "orders.unknown.v1"
        };
    }
}
