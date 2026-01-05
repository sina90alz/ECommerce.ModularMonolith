using System;
using Products.Domain.Entities;

namespace Products.Application.Interfaces;

public interface IProductRepository
{
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
    
    Task<Product?> GetByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default);
}
