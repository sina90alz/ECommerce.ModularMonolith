namespace Products.Infrastructure.Inbox;

public class InboxMessage
{
    public Guid Id { get; set; } // MessageId from RabbitMQ
    public DateTime ReceivedAtUtc { get; set; }
    public DateTime? ProcessedAtUtc { get; set; }
    public string Type { get; set; } = null!;
}
