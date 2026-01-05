using MediatR;

namespace Orders.Application.Commands.CreateOrder;

public record CreateOrderCommand(Guid ProductId) : IRequest<Guid>;
