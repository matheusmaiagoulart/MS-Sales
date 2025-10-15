using FluentResults;
using MediatR;
using Stock.Application.Products.Commands.CreateProduct;


namespace Stock.Application.Products.Commands.CreateProduct;

public class CreateProductCommand() : IRequest<Result<CreateProductResponse>>
{
    public string Name { get; init; }
    public string Description { get; init; }
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }

    public CreateProductCommand(string name, string description, decimal price, int stockQuantity) : this()
    {
        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
    }
}
