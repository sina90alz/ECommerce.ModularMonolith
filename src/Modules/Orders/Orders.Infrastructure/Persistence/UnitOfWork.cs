using Orders.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly OrdersDbContext _context;
    private readonly DomainEventsDispatcher _domainEventsDispatcher;

    public UnitOfWork(
        OrdersDbContext context,
        DomainEventsDispatcher domainEventsDispatcher)
    {
        _context = context;
        _domainEventsDispatcher = domainEventsDispatcher;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        // Persist changes
        await _context.SaveChangesAsync(cancellationToken);

        // Dispatch domain events AFTER commit
        await _domainEventsDispatcher.DispatchAsync(_context, cancellationToken);
    }
}
