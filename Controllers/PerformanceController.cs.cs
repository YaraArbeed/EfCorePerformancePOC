// Controllers/PerformanceController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EfCorePerformanceDemo.Data;

namespace EfCorePerformanceDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PerformanceController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<PerformanceController> _logger;

    public PerformanceController(AppDbContext context, ILogger<PerformanceController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("CompareQueries")]
    public async Task<IActionResult> CompareQueries(CancellationToken token)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Inefficient: loads all products, then filters in memory
        var allProducts = await _context.Products.ToListAsync(token);
        var expensiveInMemory = allProducts.Where(p => p.Price > 100).ToList();
        stopwatch.Stop();
        _logger.LogInformation("In-memory filter: {Time} ms", stopwatch.ElapsedMilliseconds);

        stopwatch.Restart();

        // Efficient: filters in database
        var expensiveInDb = await _context.Products
            .Where(p => p.Price > 100)
            .AsNoTracking()
            .ToListAsync(token);
        stopwatch.Stop();
        _logger.LogInformation("Database filter: {Time} ms", stopwatch.ElapsedMilliseconds);

        return Ok(new
        {
            InMemoryCount = expensiveInMemory.Count,
            InDbCount = expensiveInDb.Count
        });
    }

    [HttpGet("CompareTracking")]
    public async Task<IActionResult> CompareTracking(CancellationToken token)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var tracked = await _context.Products.Include(p => p.Category).ToListAsync(token);
        stopwatch.Stop();
        var trackedTime = stopwatch.ElapsedMilliseconds;

        stopwatch.Restart();
        var notTracked = await _context.Products.AsNoTracking().Include(p => p.Category).ToListAsync(token);
        stopwatch.Stop();
        var notTrackedTime = stopwatch.ElapsedMilliseconds;

        return Ok(new
        {
            TrackedTime = trackedTime,
            NoTrackingTime = notTrackedTime
        });
    }

    [HttpGet("longquery")]
    public async Task<IActionResult> GetLongQuery(CancellationToken cancellationToken)
    {
        // For clarity: cancellationToken is bound to HttpContext.RequestAborted
        _logger.LogInformation("Request started. IsCancellationRequested={IsCancelled}", cancellationToken.IsCancellationRequested);

        try
        {
            // Wait but allow cancellation
            await Task.Delay(5000, cancellationToken); // will throw if client cancels

            var products = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .OrderBy(p => p.Id)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Request finished normally.");
            return Ok(products);
        }
        catch (OperationCanceledException)
        {
            // Log server-side - client already disconnected so it won't receive this response.
            _logger.LogWarning("Request was cancelled by client. RequestAborted.IsCancellationRequested={IsCancelled}",
                HttpContext.RequestAborted.IsCancellationRequested);

            // Attempt to return 499 — note: client will likely not receive it if connection is closed.
            return StatusCode(499, "Query was cancelled by the client.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in longquery");
            throw;
        }
    }



}
