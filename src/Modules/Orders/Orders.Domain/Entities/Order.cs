using Orders.Domain.Common;
using Orders.Domain.Events;
using Orders.Domain.ValueObjects;

namespace Orders.Domain.Entities;

public class Order : Entity
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal ProductPrice { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public OrderStatus Status { get; private set; }

    private Order() { } // For EF Core

    private Order(Guid id, Guid productId, decimal productPrice)
    {
        Id = id;
        ProductId = productId;
        ProductPrice = productPrice;
        CreatedAt = DateTime.UtcNow;
        Status = OrderStatus.Created;
    }

    public static Order Create(Guid productId, decimal productPrice)
    {
        var order = new Order(
            Guid.NewGuid(),
            productId,
            productPrice
        );

        order.AddDomainEvent(new OrderCreatedDomainEvent(
            order.Id,
            order.ProductId,
            order.ProductPrice,
            order.CreatedAt
        ));

        return order;
    }

    public void MarkAsPaid()
    {
        if (Status != OrderStatus.Created)
            throw new InvalidOperationException(
                $"Order in status {Status} cannot be paid.");

        Status = OrderStatus.Paid;
        PaidAt = DateTime.UtcNow;

        AddDomainEvent(new OrderPaidDomainEvent(
            Id,
            PaidAt.Value
        ));
    }

    public void Cancel()
    {
        if (Status != OrderStatus.Created)
            throw new InvalidOperationException(
                $"Order in status {Status} cannot be cancelled.");

        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;

        AddDomainEvent(new OrderCancelledDomainEvent(
            Id,
            CancelledAt.Value
        ));
    }
}
