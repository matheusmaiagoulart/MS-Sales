namespace Stock.Application.Products.Queries.StockValidation;

public class OrderItemDTO
{
    public Guid IdProduct { get; init; }
    public int Quantity { get; init; }
}