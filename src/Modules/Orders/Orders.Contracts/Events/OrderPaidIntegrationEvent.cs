namespace Orders.Contracts.Events;

public sealed record OrderPaidIntegrationEvent(
    Guid OrderId,
    Guid ProductId,
    decimal ProductPrice,
    int Quantity
) : IntegrationEvent;