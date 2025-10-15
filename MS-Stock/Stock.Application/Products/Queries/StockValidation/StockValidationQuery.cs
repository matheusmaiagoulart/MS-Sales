using FluentResults;
using MediatR;

namespace Stock.Application.Products.Queries.StockValidation;

public class StockValidationQuery() : IRequest<Result<StockValidationResponse>>
{
    public Guid IdOrder { get; init; }
    public List<OrderItemDTO> Items { get; init; } = new();
    
    
    public StockValidationQuery(Guid idOrder, List<OrderItemDTO> orderItems) : this()
    {
        IdOrder = idOrder;
        Items = orderItems;
    }
}