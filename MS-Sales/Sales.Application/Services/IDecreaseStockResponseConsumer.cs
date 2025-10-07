using Sales.Domain.DTOs;

namespace Sales.Application.Services;

public interface IDecreaseStockResponseConsumer
{
    Task<UpdateStockResponse> Consume(string queue, Guid idOrder, int tokenSourceTimeout);
}