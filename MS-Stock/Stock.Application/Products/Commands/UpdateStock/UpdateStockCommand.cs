using FluentResults;
using MediatR;

namespace Stock.Application.Products.Commands.UpdateStock;

public record UpdateStockCommand(int Quantity, Guid IdProduct) : IRequest<Result>;