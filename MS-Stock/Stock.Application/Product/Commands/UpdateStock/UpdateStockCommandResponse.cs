namespace Stock.Application.Product.Commands.UpdateStock;

public class UpdateStockCommandResponse
{
    public Guid IdOrder { get; init; }
    public bool IsSaleSuccess { get; set; }

    public UpdateStockCommandResponse(Guid idOrder, bool isSaleSuccess)
    {
        IdOrder = idOrder;
        IsSaleSuccess = isSaleSuccess;
    }
}