namespace Stock.Application.Product.Queries.StockValidation;

public class OrderItemDto
{
    public Guid IdProduct { get; init; }
    public int Quantity { get; init; }
}