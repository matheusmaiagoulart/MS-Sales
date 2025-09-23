using FluentResults;
using MediatR;

namespace Stock.Application.Products.Commands.UpdateProduct;

public record UpdateProductCommand(Guid Id, string? Name, string? Description, decimal? Price)
    : IRequest<Result<UpdateProductResponse>>;