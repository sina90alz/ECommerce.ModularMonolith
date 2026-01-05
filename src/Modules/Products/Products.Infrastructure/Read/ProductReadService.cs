using Products.Application.Interfaces;
using Products.Contracts;

namespace Products.Infrastructure.Read;

public class ProductReadService : IProductReadService
{
    private readonly IProductRepository _repository;

    public ProductReadService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductSnapshotDto?> GetByIdAsync(Guid productId)
    {
        var product = await _repository.GetByIdAsync(productId);

        if (product is null)
            return null;

        return new ProductSnapshotDto(
            product.Id,
            product.Name,
            product.Price);
    }
}
