namespace Orders.Contracts.Events;

public sealed record OrderCancelledIntegrationEvent(
    Guid OrderId,
    Guid ProductId,
    decimal ProductPrice,
    int Quantity
) : IntegrationEvent;