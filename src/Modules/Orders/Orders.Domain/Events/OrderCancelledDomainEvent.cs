using Orders.Domain.Common;

namespace Orders.Domain.Events;

public sealed record OrderCancelledDomainEvent(
    Guid OrderId,
    DateTime CancelledAtUtc
) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
