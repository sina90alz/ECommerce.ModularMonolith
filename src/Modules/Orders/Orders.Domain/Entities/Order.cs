namespace Orders.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string Status { get; private set; }

    private Order() { } // EF

    public Order(Guid id)
    {
        Id = id;
        CreatedAt = DateTime.UtcNow;
        Status = "Created";
    }

    public void MarkAsPaid()
    {
        Status = "Paid";
    }
}
