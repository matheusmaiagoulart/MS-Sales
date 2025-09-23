using Sales.Domain.Models;

namespace Sales.Application.Orders.Commands.CreateOrder;

public record CreateOrderResponse (
    int IdOrder,
    List<OrdemItem> OrdemItems,
    decimal TotalAmount,
    StatusSale Status,
    DateTime CreatedAt
);