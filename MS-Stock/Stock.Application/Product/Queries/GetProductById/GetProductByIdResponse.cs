namespace Stock.Application.Product.Queries.GetProductById;

public record GetProductByIdResponse(
    Guid IdProduct,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);