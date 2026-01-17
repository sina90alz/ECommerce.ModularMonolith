using MediatR;
using Orders.Application.Interfaces;
using Orders.Application.Outbox;
using Orders.Contracts.Events;

namespace Orders.Application.Commands.CancelOrder;

public sealed class CancelOrderHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxWriter _outbox;

    public CancelOrderHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IOutboxWriter outbox)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _outbox = outbox;
    }

    public async Task Handle(CancelOrderCommand request, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, ct)
            ?? throw new InvalidOperationException("Order not found.");

        order.Cancel();

        // Build integration event directly
        var evt = new OrderCancelledIntegrationEvent(
            OrderId: order.Id,
            ProductId: order.ProductId,
            ProductPrice: order.ProductPrice,
            Quantity: 1
        );

        _outbox.Add(evt);

        await _unitOfWork.CommitAsync(ct);
    }
}
