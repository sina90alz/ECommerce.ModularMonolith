using FluentAssertions;
using NetArchTest.Rules;
using Products.Domain.Entities;
using Products.Application.Commands.CreateProduct;
using Products.Infrastructure.Persistence;

namespace Architecture.Tests;

public class ProductsModuleArchitectureTests
{
    private const string ProductsDomainNamespace = "Products.Domain";
    private const string ProductsApplicationNamespace = "Products.Application";
    private const string ProductsInfrastructureNamespace = "Products.Infrastructure";

    private const string OrdersDomainNamespace = "Orders.Domain";
    private const string OrdersApplicationNamespace = "Orders.Application";
    private const string OrdersInfrastructureNamespace = "Orders.Infrastructure";

    [Fact]
    public void Products_Domain_Should_Not_Depend_On_Other_Layers()
    {
        var result = Types.InAssembly(typeof(Product).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                ProductsApplicationNamespace,
                ProductsInfrastructureNamespace,
                OrdersDomainNamespace,
                OrdersApplicationNamespace,
                OrdersInfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Products.Domain must be completely independent");
    }

    [Fact]
    public void Products_Application_Should_Not_Depend_On_Infrastructure_Or_Other_Modules()
    {
        var result = Types.InAssembly(typeof(CreateProductCommand).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                ProductsInfrastructureNamespace,
                OrdersDomainNamespace,
                OrdersApplicationNamespace,
                OrdersInfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Products.Application must not depend on Infrastructure or other modules");
    }

    [Fact]
    public void Products_Infrastructure_Should_Not_Depend_On_Other_Modules()
    {
        var result = Types.InAssembly(typeof(ProductsDbContext).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                OrdersDomainNamespace,
                OrdersApplicationNamespace,
                OrdersInfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Products.Infrastructure must not depend on other modules");
    }
}
