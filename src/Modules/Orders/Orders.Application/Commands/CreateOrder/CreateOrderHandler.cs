using MediatR;
using Orders.Application.Interfaces;
using Orders.Domain.Entities;
using Products.Contracts;

namespace Orders.Application.Commands.CreateOrder;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IProductReadService _products;
    private readonly IOrderRepository _orders;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderHandler(
        IProductReadService products,
        IOrderRepository orders,
        IUnitOfWork unitOfWork)
    {
        _products = products;
        _orders = orders;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var product = await _products.GetByIdAsync(request.ProductId);

        if (product is null)
            throw new InvalidOperationException("Product not found");

        var order = Order.Create(product.Id, product.Price);

        await _orders.AddAsync(order, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return order.Id;
    }
}
