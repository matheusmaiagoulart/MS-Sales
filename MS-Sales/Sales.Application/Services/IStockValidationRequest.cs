using FluentResults;
using Sales.Application.Orders.Commands.CreateOrder;
using Sales.Domain.DTOs;

namespace Sales.Application.Services;

public interface IStockValidationRequest
{
    Task<Result<RequestCreateOrderValidationResponse>> SendStockValidationRequest(CreateOrderCommand request, Guid idOrder);
}