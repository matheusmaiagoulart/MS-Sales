using FluentResults;
using MediatR;
using Stock.Application.Products.Queries.StockValidation;

namespace Stock.Application.Products.Commands.UpdateStock;

public class UpdateStockCommand() : IRequest<Result<UpdateStockCommandResponse>>
{
    public Guid IdOrder { get; init; }
    public List<OrderItemDTO> Items { get; set; } = new();
    
    public UpdateStockCommand(Guid idOrder, List<OrderItemDTO> items) : this()
    {
        IdOrder = idOrder;
        Items = items;
    }
    
}