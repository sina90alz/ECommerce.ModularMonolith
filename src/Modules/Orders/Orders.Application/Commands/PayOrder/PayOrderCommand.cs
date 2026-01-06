using MediatR;

namespace Orders.Application.Commands.PayOrder;

public sealed record PayOrderCommand(Guid OrderId) : IRequest;
