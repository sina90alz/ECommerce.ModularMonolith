using MediatR;
using Orders.Domain.Entities;
using Orders.Application.Interfaces;

namespace Orders.Application.Commands.CreateOrder;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _repository;

    public CreateOrderHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order(Guid.NewGuid());
        await _repository.AddAsync(order);
        return order.Id;
    }
}
