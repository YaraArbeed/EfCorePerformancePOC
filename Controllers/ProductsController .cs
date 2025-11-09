// Controllers/ProductsController.cs
using EfCorePerformanceDemo.Data;
using EfCorePerformanceDemo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace EfCorePerformanceDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(AppDbContext context, ILogger<ProductsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET (No Tracking for performance)
    [HttpGet]
    [EnableRateLimiting("fixed")]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll(CancellationToken token)
    {
        return await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .AsSplitQuery() // Performance optimization
            .ToListAsync(token);
    }

    // GET by Id (tracked)
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetById(int id, CancellationToken token)
    {
        var product = await _context.Products.FindAsync(new object[] { id }, token);
        if (product == null) return NotFound();
        return product;
    }

    // POST (Create)
    [HttpPost]
    public async Task<ActionResult<Product>> Create(Product product, CancellationToken token)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync(token);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    // PUT (Update with concurrency check)
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Product updated, CancellationToken token)
    {
        if (id != updated.Id) return BadRequest();

        _context.Entry(updated).Property("CreatedAt").IsModified = false;

        _context.Entry(updated).OriginalValues["RowVersion"] = updated.RowVersion;

        try
        {
            _context.Update(updated);
            await _context.SaveChangesAsync(token);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Products.Any(p => p.Id == id)) return NotFound();
            throw;
        }

        return NoContent();
    }

    // DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken token)
    {
        var product = await _context.Products.FindAsync(new object[] { id }, token);
        if (product == null) return NotFound();

        _context.Products.Remove(product);
        await _context.SaveChangesAsync(token);
        return NoContent();
    }
}
