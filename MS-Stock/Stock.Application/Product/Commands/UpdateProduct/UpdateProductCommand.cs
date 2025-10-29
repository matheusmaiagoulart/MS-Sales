using FluentResults;
using MediatR;

namespace Stock.Application.Product.Commands.UpdateProduct;

public record UpdateProductCommand(): IRequest<Result<UpdateProductResponse>>
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public decimal? Price { get; init; }
    public int StockQuantity { get; init; }

    public UpdateProductCommand(Guid id, string name, string description, decimal price, int stockQuantity) : this()
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
    }
}
