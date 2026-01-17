namespace Orders.Contracts.Events;

public abstract record IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOnUtc { get; init; } = DateTime.UtcNow;
    public int Version { get; init; } = 1;
}
