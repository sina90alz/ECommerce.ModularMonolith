using MediatR;

namespace Products.Application.Commands.CreateProduct;

public record CreateProductCommand(string Name, decimal Price, int QuantityOnHand) : IRequest<Guid>;
