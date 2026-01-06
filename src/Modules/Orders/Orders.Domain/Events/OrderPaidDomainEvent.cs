using Orders.Domain.Common;

namespace Orders.Domain.Events;

public sealed record OrderPaidDomainEvent(
    Guid OrderId,
    DateTime PaidAtUtc
) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
