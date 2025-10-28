namespace Stock.Application.Products.Queries.StockValidation;

public record StockValidationResponse
{
    public Guid IdOrder { get; init; }
    public bool Available { get; set; }
    public decimal TotalAmount { get; set; }
    
    public StockValidationResponse(Guid idOrder, bool available, decimal totalAmount)
    {
        IdOrder = idOrder;
        Available = available;
        TotalAmount = totalAmount;
    }
}