using MediatR;
using Products.Application.Interfaces;
using Products.Domain.Entities;

namespace Products.Application.Commands.CreateProduct;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _repository;

    public CreateProductHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product(Guid.NewGuid(), request.Name, request.Price);
        await _repository.AddAsync(product, cancellationToken);
        return product.Id;
    }
}
