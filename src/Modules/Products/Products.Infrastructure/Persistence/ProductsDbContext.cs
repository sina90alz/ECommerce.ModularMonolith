using Microsoft.EntityFrameworkCore;
using Products.Domain.Entities;
using Products.Infrastructure.Inbox;

namespace Products.Infrastructure.Persistence;

public class ProductsDbContext : DbContext
{
    public ProductsDbContext(DbContextOptions<ProductsDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InboxMessage>()
            .HasKey(x => x.Id);

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
}
