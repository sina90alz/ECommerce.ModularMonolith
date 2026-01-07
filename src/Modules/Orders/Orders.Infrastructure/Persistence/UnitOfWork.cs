using Orders.Domain.Common;
using Orders.Infrastructure.Outbox;
using Orders.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly OrdersDbContext _context;
    private readonly OutboxWriter _outboxWriter;

    public UnitOfWork(
        OrdersDbContext context,
        OutboxWriter outboxWriter)
    {
        _context = context;
        _outboxWriter = outboxWriter;
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        // Collect domain events BEFORE SaveChanges
        var domainEvents = _context.ChangeTracker
            .Entries<Entity>()
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();

        // Write to outbox (same transaction)
        _outboxWriter.Write(_context, domainEvents);

        // Clear domain events
        foreach (var entity in _context.ChangeTracker.Entries<Entity>())
            entity.Entity.ClearDomainEvents();

        // Commit everything atomically
        await _context.SaveChangesAsync(ct);
    }
}