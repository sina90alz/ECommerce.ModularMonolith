using Orders.Domain.Common;
using Orders.Domain.Events;

namespace Orders.Domain.Entities;

public class Order : Entity
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal ProductPrice { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string Status { get; private set; } = null!;

    private Order() { } // For EF Core

    private Order(Guid id, Guid productId, decimal productPrice)
    {
        Id = id;
        ProductId = productId;
        ProductPrice = productPrice;
        CreatedAt = DateTime.UtcNow;
        Status = "Created";
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
}
