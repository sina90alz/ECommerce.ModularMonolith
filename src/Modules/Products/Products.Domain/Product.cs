namespace Products.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public decimal Price { get; private set; }

    // Stock / inventory
    public int QuantityOnHand { get; private set; }

    private Product() { } // EF

    public Product(Guid id, string name, decimal price, int quantityOnHand)
    {
        if (quantityOnHand < 0) throw new ArgumentOutOfRangeException(nameof(quantityOnHand));

        Id = id;
        Name = name;
        Price = price;
        QuantityOnHand = quantityOnHand;
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        QuantityOnHand += quantity;
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (QuantityOnHand < quantity) throw new InvalidOperationException("Insufficient stock.");

        QuantityOnHand -= quantity;
    }

    public void ChangePrice(decimal newPrice)
    {
        if (newPrice <= 0) throw new ArgumentOutOfRangeException(nameof(newPrice));
        Price = newPrice;
    }
}
