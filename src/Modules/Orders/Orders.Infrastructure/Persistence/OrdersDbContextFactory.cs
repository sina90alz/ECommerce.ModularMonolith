using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Orders.Infrastructure.Persistence;

public class OrdersDbContextFactory
    : IDesignTimeDbContextFactory<OrdersDbContext>
{
    public OrdersDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrdersDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=OrdersDb;Trusted_Connection=True;TrustServerCertificate=True");

        return new OrdersDbContext(optionsBuilder.Options);
    }
}
