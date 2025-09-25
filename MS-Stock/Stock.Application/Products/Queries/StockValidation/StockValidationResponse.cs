namespace Stock.Application.Products.Queries.StockValidation;

public record StockValidationResponse
{
    public bool Available { get; set; }
    public decimal TotalAmount { get; set; }
}