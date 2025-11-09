// Services/OrderService.cs
using EfCorePerformanceDemo.Data;
using EfCorePerformanceDemo.Models;
using EfCorePerformanceDemo.Events;

namespace EfCorePerformanceDemo.Services;

public class OrderService
{
    private readonly AppDbContext _context;
    public event EventHandler<OrderCreatedEventArgs>? OrderCreated;

    public OrderService(AppDbContext context) => _context = context;

    public async Task<Order> CreateOrderAsync(Order order, CancellationToken token)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(token);
        OnOrderCreated(order);
        return order;
    }

    protected virtual void OnOrderCreated(Order order)
    {
        OrderCreated?.Invoke(this, new OrderCreatedEventArgs(order));
    }
}
