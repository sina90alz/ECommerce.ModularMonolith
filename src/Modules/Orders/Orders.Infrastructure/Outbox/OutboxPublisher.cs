using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Orders.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Orders.Infrastructure.Outbox;

public class OutboxPublisher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public OutboxPublisher(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

            var messages = await db.OutboxMessages
                .Where(x => x.ProcessedOnUtc == null)
                .OrderBy(x => x.OccurredOnUtc)
                .Take(20)
                .ToListAsync(ct);

            foreach (var message in messages)
            {
                try
                {
                    // Later: publish to Kafka / RabbitMQ
                    Console.WriteLine($"Publishing {message.Type}");

                    message.ProcessedOnUtc = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    message.Error = ex.Message;
                }
            }

            await db.SaveChangesAsync(ct);
            await Task.Delay(2000, ct);
        }
    }
}
