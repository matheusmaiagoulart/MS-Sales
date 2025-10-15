using FluentResults;
using Sales.Domain.DTOs;

namespace Sales.Application.Services;

public interface IDecreaseStockResponseConsumer
{
    Task<Result<UpdateStockResponse>> Consume(string queue, Guid idOrder, int timeoutSeconds);
}