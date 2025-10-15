using FluentResults;
using MediatR;
using Sales.Domain.Models;

namespace Sales.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommand : IRequest<Result<CreateOrderResponse>>
{
    public Guid IdOrder { get; private set; }
    public List<OrdemItem> Items { get; set; }
    
    public void setIdOrder(Guid idOrder)
    {
        IdOrder = idOrder;
    }
    public Guid getIdOrder()
    {
        return IdOrder;
    }
}
