using Orders.Application.Outbox;
using Orders.Contracts.Events;
using Orders.Domain.Common;
using Orders.Infrastructure.Persistence;

namespace Orders.Infrastructure.Outbox;

public class OutboxWriter : IOutboxWriter
{    
    private readonly OrdersDbContext _db;

    public OutboxWriter(OrdersDbContext db)
    {
        _db = db;
    }

    public void Write(
        OrdersDbContext context,
        IReadOnlyCollection<IDomainEvent> domainEvents)
    {
        foreach (var domainEvent in domainEvents)
        {
            var outboxMessage = OutboxMessage.FromDomainEvent(domainEvent);
            context.OutboxMessages.Add(outboxMessage);
        }
    }
    
    public void Add(IntegrationEvent evt)
    {
        // IMPORTANT: do NOT call SaveChanges here.
        // This must be part of the same transaction as the Order update.
        _db.OutboxMessages.Add(OutboxMessage.FromIntegrationEvent(evt));
    }
}
