using MediatR;
using Microsoft.EntityFrameworkCore;
using Orders.Domain.Common;

namespace Orders.Infrastructure.Persistence;

public sealed class DomainEventsDispatcher
{
    private readonly IMediator _mediator;

    public DomainEventsDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task DispatchAsync(DbContext dbContext, CancellationToken ct)
    {
        // Grab tracked entities that have events
        var entitiesWithEvents = dbContext.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var events = entitiesWithEvents
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // Clear first to avoid re-dispatch on retry paths
        foreach (var entity in entitiesWithEvents)
            entity.ClearDomainEvents();

        foreach (var domainEvent in events)
            await _mediator.Publish(domainEvent, ct);
    }
}
