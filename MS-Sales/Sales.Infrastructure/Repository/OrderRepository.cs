using Sales.Domain.Interfaces;
using Sales.Domain.Models;
using Sales.Infrastructure.Data.Context;

namespace Sales.Infrastructure.Repository;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;
    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task CreateOrder(Order order)
    {
        await _context.Orders.AddAsync(order);
    }

    public async Task UpdateStatusOrder(Order order)
    {
        await _context.Orders.AddAsync(order);
    }

    public async Task<Order?> GetOrderById(int orderId)
    {
        return await _context.Orders
            .FindAsync(orderId);
    }
    
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    
}