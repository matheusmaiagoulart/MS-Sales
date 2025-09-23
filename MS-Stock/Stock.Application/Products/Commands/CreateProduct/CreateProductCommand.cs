using FluentResults;
using MediatR;
using Stock.Application.Products.Commands.CreateProduct;


namespace Stock.Application.Products.Commands.CreateProduct;

public record CreateProductCommand(string Name, string Description, decimal Price, int StockQuantity)
    : IRequest<Result<CreateProductResponse>>;