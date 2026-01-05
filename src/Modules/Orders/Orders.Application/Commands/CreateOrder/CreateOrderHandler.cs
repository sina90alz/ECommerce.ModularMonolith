using MediatR;
using Orders.Application.Interfaces;
using Orders.Domain.Entities;
using Products.Contracts;

namespace Orders.Application.Commands.CreateOrder;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IProductReadService _products;
    private readonly IOrderRepository _orders;

    public CreateOrderHandler(
        IProductReadService products,
        IOrderRepository orders)
    {
        _products = products;
        _orders = orders;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var product = await _products.GetByIdAsync(request.ProductId);

        if (product is null)
            throw new InvalidOperationException("Product not found");

        var order = new Order(
            Guid.NewGuid(),
            product.Id,
            product.Price);

        await _orders.AddAsync(order, cancellationToken);

        return order.Id;
    }
}
