// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using EfCorePerformanceDemo.Models;

namespace EfCorePerformanceDemo.Data;

public class AppDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Order> Orders => Set<Order>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Shadow property example
        modelBuilder.Entity<Product>()
            .Property<DateTime>("CreatedAt")
            .HasDefaultValueSql("GETDATE()");

        // Enum -> string value converter
        modelBuilder.Entity<Product>()
            .Property(p => p.Status)
            .HasConversion(
                v => v.ToString(),
                v => (ProductStatus)Enum.Parse(typeof(ProductStatus), v)
            );
        // Data/AppDbContext.cs (inside OnModelCreating)
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Electronics" },
            new Category { Id = 2, Name = "Books" }
        );


        base.OnModelCreating(modelBuilder);
    }
}
