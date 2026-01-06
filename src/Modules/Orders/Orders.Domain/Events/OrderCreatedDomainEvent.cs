using Orders.Domain.Common;

namespace Orders.Domain.Events;

public sealed record OrderCreatedDomainEvent(
    Guid OrderId,
    Guid ProductId,
    decimal ProductPrice,
    DateTime CreatedAtUtc
) : IDomainEvent
{
    public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
}
