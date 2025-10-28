using FluentResults;
using Sales.Application.Orders.Commands.CreateOrder;
using Sales.Domain.DTOs;

namespace Sales.Application.Services;

public interface IStockDecreaseRequest
{
    Task<Result<UpdateStockResponse>> SendDecreaseStockRequest(CreateOrderCommand request, Guid idOrder);
}