using MediatR;
using Orders.Domain.Entities;
using Products.Contracts;

namespace Orders.Application.Commands.CreateOrder;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IProductReadService _products;

    public CreateOrderHandler(IProductReadService products)
    {
        _products = products;
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

        return order.Id;
    }
}
