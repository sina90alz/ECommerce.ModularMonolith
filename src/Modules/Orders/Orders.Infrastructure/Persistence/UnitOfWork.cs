using Orders.Domain.Common;
using Orders.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly OrdersDbContext _context;

    public UnitOfWork(
        OrdersDbContext context)
    {
        _context = context;
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        // Commit everything atomically
        await _context.SaveChangesAsync(ct);
    }
}