namespace Orders.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public decimal ProductPrice { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string Status { get; private set; } = null!;

    private Order() { } // For EF

    public Order(Guid id, Guid productId, decimal productPrice)
    {
        Id = id;
        ProductId = productId;
        ProductPrice = productPrice;
        CreatedAt = DateTime.UtcNow;
        Status = "Created";
    }
}
