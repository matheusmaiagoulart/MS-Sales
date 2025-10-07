using FluentResults;
using MediatR;
using Stock.Application.Products.Queries.StockValidation;

namespace Stock.Application.Products.Commands.UpdateStock;

public record UpdateStockCommand(Guid IdOrder, List<OrderItemDTO> Items) : IRequest<Result<UpdateStockCommandResponse>>;