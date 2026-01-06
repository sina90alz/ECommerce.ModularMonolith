using MediatR;

namespace Orders.Domain.Common;

// Marker for domain events (still in-process via MediatR)
public interface IDomainEvent : INotification
{
    DateTime OccurredOnUtc { get; }
}
