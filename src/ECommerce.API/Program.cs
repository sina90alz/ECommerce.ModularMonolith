using ECommerce.API.Behaviors;
using FluentValidation;
using MediatR;
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

builder.Services.AddValidatorsFromAssembly(
    typeof(CreateOrderCommand).Assembly);

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));

var app = builder.Build();

app.MapControllers();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 400;
        context.Response.ContentType = "application/json";

        var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;

        if (exception is ValidationException validationException)
        {
            var errors = validationException.Errors
                .Select(e => new { e.PropertyName, e.ErrorMessage });

            await context.Response.WriteAsJsonAsync(errors);
        }
    });
});

app.Run();
