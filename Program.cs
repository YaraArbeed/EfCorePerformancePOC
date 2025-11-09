using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using EfCorePerformanceDemo.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.AddFixedWindowLimiter("fixed", options =>
    {
        options.PermitLimit = 1;           // 1 request
        options.Window = TimeSpan.FromSeconds(5); // per 5 seconds
        options.QueueLimit = 0;            // no queuing
    });
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Logging.ClearProviders();  // Remove default logging (optional)
builder.Logging.AddConsole();      // Log to console
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseRateLimiter();
app.MapControllers();
// Minimal API endpoint using the "fixed" policy
app.MapGet("/products", async (CancellationToken token) =>
{
    return new[]
    {
        new { Id = 1, Name = "Product A" },
        new { Id = 2, Name = "Product B" }
    };
})
.RequireRateLimiting("fixed"); // apply named rate limiting policy
app.Run();
