using FluentResults;
using MediatR;
using Stock.Application.Product.Queries.StockValidation;

namespace Stock.Application.Product.Commands.UpdateStock;

public class UpdateStockCommand() : IRequest<Result<UpdateStockCommandResponse>>
{
    public Guid IdOrder { get; init; }
    public List<OrderItemDto> Items { get; set; } = new();
    
    public UpdateStockCommand(Guid idOrder, List<OrderItemDto> items) : this()
    {
        IdOrder = idOrder;
        Items = items;
    }
    
}