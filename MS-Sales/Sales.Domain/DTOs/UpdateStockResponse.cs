namespace Sales.Domain.DTOs;

public class UpdateStockResponse
{
    public Guid IdOrder { get; init; }
    public bool IsSaleSuccess { get; set; }

    public UpdateStockResponse(Guid idOrder, bool isSaleSuccess)
    {
        IdOrder = idOrder;
        IsSaleSuccess = isSaleSuccess;
    }
}