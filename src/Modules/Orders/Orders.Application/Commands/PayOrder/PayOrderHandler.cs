using MediatR;
using Orders.Application.Interfaces;

namespace Orders.Application.Commands.PayOrder;

public sealed class PayOrderHandler : IRequestHandler<PayOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PayOrderHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(PayOrderCommand request, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, ct)
            ?? throw new InvalidOperationException("Order not found.");

        order.MarkAsPaid();

        await _unitOfWork.CommitAsync(ct);
    }
}
