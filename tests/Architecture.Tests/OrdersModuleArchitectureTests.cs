using FluentAssertions;
using NetArchTest.Rules;
using Orders.Domain.Entities;
using Orders.Application.Commands.CreateOrder;
using Orders.Infrastructure.Persistence;

namespace Architecture.Tests;

public class OrdersModuleArchitectureTests
{
    private const string OrdersDomainNamespace = "Orders.Domain";
    private const string OrdersApplicationNamespace = "Orders.Application";
    private const string OrdersInfrastructureNamespace = "Orders.Infrastructure";

    [Fact]
    public void Orders_Domain_Should_Not_Depend_On_Other_Layers()
    {
        var result = Types.InAssembly(typeof(Order).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                OrdersApplicationNamespace,
                OrdersInfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Orders.Domain must be completely independent");
    }

    [Fact]
    public void Orders_Application_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types.InAssembly(typeof(CreateOrderCommand).Assembly)
            .ShouldNot()
            .HaveDependencyOn(OrdersInfrastructureNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Orders.Application must not depend on Infrastructure");
    }

    [Fact]
    public void Orders_Infrastructure_Should_Not_Depend_On_Other_Modules()
    {
        var result = Types.InAssembly(typeof(OrderRepository).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Products",
                "Customers",
                "Payments")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "Orders.Infrastructure must not depend on other modules");
    }

}
