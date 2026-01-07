using Orders.Domain.Common;
using Orders.Infrastructure.Persistence;

namespace Orders.Infrastructure.Outbox;

public class OutboxWriter
{
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
}
