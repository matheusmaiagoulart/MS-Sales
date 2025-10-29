namespace Stock.Application.Product.Commands.CreateProduct;

public record CreateProductResponse(
    Guid IdProduct,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    DateTime CreatedAt
);