using Orders.Domain.Entities;

namespace Orders.Application.Interfaces;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken = default);
}
