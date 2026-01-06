using MediatR;

namespace Orders.Application.Commands.CancelOrder;

public sealed record CancelOrderCommand(Guid OrderId) : IRequest;
