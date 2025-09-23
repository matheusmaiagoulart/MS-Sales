using Sales.Domain.Models;

namespace Sales.Domain.Interfaces;

public interface IOrderRepository
{
    Task CreateOrder(Order order);
    Task UpdateStatusOrder(Order order);
    Task<Order?> GetOrderById(int orderId);
    Task SaveChangesAsync();
}