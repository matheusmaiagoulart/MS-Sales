using FluentResults;
using MediatR;
using Sales.Domain.Models;

namespace Sales.Application.Orders.Queries.GetOrderById;

public class GetOrderById : IRequest<Result<Order>>
{
    public Guid IdOrder { get; set; }
    
    public GetOrderById(Guid idOrder)
    {
        IdOrder = idOrder;
    }
}