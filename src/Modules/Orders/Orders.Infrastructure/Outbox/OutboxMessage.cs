using System.Text.Json;

namespace Orders.Infrastructure.Outbox;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = null!;
    public string Payload { get; set; } = null!;
    public DateTime OccurredOnUtc { get; set; }
    public DateTime? ProcessedOnUtc { get; set; }
    public string? Error { get; set; }

    public static OutboxMessage FromDomainEvent(object domainEvent)
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = domainEvent.GetType().Name,
            Payload = JsonSerializer.Serialize(
                domainEvent,
                domainEvent.GetType()
            ),
            OccurredOnUtc = DateTime.UtcNow
        };
    }
}
