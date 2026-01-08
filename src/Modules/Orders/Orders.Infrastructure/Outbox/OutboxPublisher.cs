using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Orders.Infrastructure.Persistence;

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
                        // 🔜 Later: publish to RabbitMQ / Kafka
                        Console.WriteLine($"Publishing {msg.Type} ({msg.Id})");

                        msg.ProcessedOnUtc = DateTime.UtcNow;
                        msg.LastError = null;
                        msg.NextAttemptOnUtc = null;
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
}
