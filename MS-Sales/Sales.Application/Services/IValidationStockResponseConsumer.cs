using FluentResults;
using Sales.Application.Orders.Commands.CreateOrder;
using Sales.Domain.DTOs;

namespace Sales.Application.Services;

public interface IValidationStockResponseConsumer
{
    Task<Result<RequestCreateOrderValidationResponse>> Consume(string queue, Guid idOrder, int timeoutSeconds);
}