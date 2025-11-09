// Events/OrderCreatedEvent.cs
using EfCorePerformanceDemo.Models;

namespace EfCorePerformanceDemo.Events;

public class OrderCreatedEventArgs : EventArgs
{
    public Order Order { get; }
    public OrderCreatedEventArgs(Order order) => Order = order;
}
