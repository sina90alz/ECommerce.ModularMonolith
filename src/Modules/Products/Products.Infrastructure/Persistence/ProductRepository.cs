using Microsoft.EntityFrameworkCore;
using Products.Application.Interfaces;
using Products.Domain.Entities;

namespace Products.Infrastructure.Persistence;

public class ProductRepository : IProductRepository
{
    private readonly ProductsDbContext _context;

    public ProductRepository(ProductsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);
    }

        public async Task<Product?> GetByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
    }
}
