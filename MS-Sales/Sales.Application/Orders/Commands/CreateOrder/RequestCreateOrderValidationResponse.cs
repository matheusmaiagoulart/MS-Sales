namespace Sales.Application.Orders.Commands.CreateOrder;

public class RequestCreateOrderValidationResponse
{
    public Guid IdOrder { get; init; }
    public bool IsStockAvailable { get; set; }
    public decimal ValueAmount { get; set; }
}