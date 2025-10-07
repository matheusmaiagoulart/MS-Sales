using Sales.Application.Orders.Commands.CreateOrder;
using Sales.Domain.DTOs;

namespace Sales.Application.Services;

public interface IValidationStockResponseConsumer
{
    Task<RequestCreateOrderValidationResponse> Consume(string queue, Guid idOrder, int tokenSourceTimeout);
}