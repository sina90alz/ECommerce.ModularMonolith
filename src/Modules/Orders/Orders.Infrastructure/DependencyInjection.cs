using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Orders.Application.Interfaces;
using Orders.Application.Outbox;
using Orders.Infrastructure.Messaging;
using Orders.Infrastructure.Outbox;
using Orders.Infrastructure.Persistence;

namespace Orders.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrdersInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<OrdersDbContext>(opt =>
            opt.UseSqlServer(connectionString));

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IOutboxWriter, OutboxWriter>();
        services.AddScoped<OutboxWriter>();
        services.AddSingleton<IMessageBus>( _ => new RabbitMqMessageBus(hostName: "localhost"));
        services.AddHostedService<OutboxPublisher>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<DomainEventsDispatcher>();

        return services;
    }
}
