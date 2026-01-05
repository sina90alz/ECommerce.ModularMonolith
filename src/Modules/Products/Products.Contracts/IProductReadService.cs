namespace Products.Contracts;

public interface IProductReadService
{
    Task<ProductSnapshotDto?> GetByIdAsync(Guid productId);
}
