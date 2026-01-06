using MediatR;
using Orders.Domain.Events;

namespace Orders.Application.Events;

public sealed class OrderPaidHandler
    : INotificationHandler<OrderPaidDomainEvent>
{
    public Task Handle(
        OrderPaidDomainEvent notification,
        CancellationToken cancellationToken)
    {
        // - create audit record
        // - update read model
        // - prepare invoice
        // - enqueue outbox message later

        return Task.CompletedTask;
    }
}
