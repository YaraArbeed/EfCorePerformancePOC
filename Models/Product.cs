// Models/Product.cs
using System.ComponentModel.DataAnnotations;

namespace EfCorePerformanceDemo.Models;

public enum ProductStatus { Active, Inactive }

public class Product
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public ProductStatus Status { get; set; } = ProductStatus.Active;

    [Timestamp] // Concurrency token
    public byte[]? RowVersion { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}
