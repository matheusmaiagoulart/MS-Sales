namespace Stock.Application.Products.Queries.GetProductById;

public record GetProductByIdResponse(
    Guid IdProduct,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);