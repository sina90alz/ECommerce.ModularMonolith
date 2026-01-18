using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Products.Application.Interfaces;
using Products.Contracts;
using Products.Infrastructure.Inbox;
using Products.Infrastructure.Messaging;
using Products.Infrastructure.Persistence;
using Products.Infrastructure.Read;

namespace Products.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProductsInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<ProductsDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IProductRepository, ProductRepository>();     
        services.AddScoped<IProductReadService, ProductReadService>();
        services.AddSingleton<IMessageBus, RabbitMqMessageBus>();
        services.AddHostedService<OrdersEventsConsumer>();

        return services;
    }
}
