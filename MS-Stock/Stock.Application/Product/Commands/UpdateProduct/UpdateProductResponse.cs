namespace Stock.Application.Product.Commands.UpdateProduct;

public record UpdateProductResponse(
    Guid IdProduct,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);