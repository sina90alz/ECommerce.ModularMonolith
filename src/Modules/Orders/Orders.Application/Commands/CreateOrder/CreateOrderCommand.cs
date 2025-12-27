using MediatR;

namespace Orders.Application.Commands.CreateOrder;

public record CreateOrderCommand : IRequest<Guid>;
