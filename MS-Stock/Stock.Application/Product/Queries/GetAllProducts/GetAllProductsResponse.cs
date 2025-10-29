namespace Stock.Application.Product.Queries.GetAllProducts;

public record GetAllProductsResponse
(
    Guid IdProduct,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);