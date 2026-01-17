using Orders.Contracts.Events;

namespace Orders.Application.Outbox;

public interface IOutboxWriter
{
    void Add(IntegrationEvent evt);
}
