using FluentResults;
using MediatR;

namespace Stock.Application.Product.Queries.StockValidation;

public class StockValidationQuery() : IRequest<Result<StockValidationResponse>>
{
    public Guid IdOrder { get; init; }
    public List<OrderItemDto> Items { get; init; } = new();
    
    public StockValidationQuery(Guid idOrder, List<OrderItemDto> orderItems) : this()
    {
        IdOrder = idOrder;
        Items = orderItems;
    }
}