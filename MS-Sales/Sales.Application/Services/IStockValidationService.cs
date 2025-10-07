using FluentResults;
using Sales.Application.Orders.Commands.CreateOrder;
using Sales.Domain.DTOs;

namespace Sales.Application.Services;

public interface IStockValidationService
{
    Task<Result<T>> ValidateStockAsync<T>(CreateOrderCommand request,
        Guid orderId, string queueName, CancellationToken cancellationToken);
}