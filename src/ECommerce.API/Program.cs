using Orders.Application.Commands.CreateOrder;
using Orders.Infrastructure;
using Products.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Orders.Application.Commands.CreateOrder.CreateOrderCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Products.Application.Commands.CreateProduct.CreateProductCommand).Assembly);
});

var ordersDbConnectionString =
    builder.Configuration.GetConnectionString("OrdersDb")
    ?? throw new InvalidOperationException("OrdersDb connection string is not configured.");

var productsDbConnectionString =
builder.Configuration.GetConnectionString("ProductsDb")
?? throw new InvalidOperationException("ProductsDb connection string is not configured.");

builder.Services.AddOrdersInfrastructure(ordersDbConnectionString);
builder.Services.AddProductsInfrastructure(productsDbConnectionString);

var app = builder.Build();

app.MapControllers();

app.Run();
