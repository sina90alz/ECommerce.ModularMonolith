using Orders.Application.Commands.CreateOrder;
using Orders.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly));

var ordersDbConnectionString =
    builder.Configuration.GetConnectionString("OrdersDb")
    ?? throw new InvalidOperationException("OrdersDb connection string is not configured.");

builder.Services.AddOrdersInfrastructure(ordersDbConnectionString);

var app = builder.Build();

app.MapControllers();

app.Run();
