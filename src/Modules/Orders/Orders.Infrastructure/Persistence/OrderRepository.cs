using Microsoft.EntityFrameworkCore;
using Orders.Application.Interfaces;
using Orders.Domain.Entities;
using Orders.Infrastructure.Persistence;

public class OrderRepository : IOrderRepository
{
    private readonly OrdersDbContext _context;

    public OrderRepository(OrdersDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        _context.Orders.Add(order);
        return Task.CompletedTask;
    }

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Orders
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }
}
