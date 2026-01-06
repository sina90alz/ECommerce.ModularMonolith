using MediatR;
using Orders.Application.Interfaces;
using Orders.Domain.Entities;

namespace Orders.Application.Commands.CancelOrder;

public sealed class CancelOrderHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrderHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CancelOrderCommand request, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, ct)
            ?? throw new InvalidOperationException("Order not found.");

        order.Cancel();

        await _unitOfWork.CommitAsync(ct);
    }
}
