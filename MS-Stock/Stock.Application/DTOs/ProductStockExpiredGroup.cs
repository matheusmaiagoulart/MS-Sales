namespace Stock.Application.DTOs;

public class ProductStockExpiredGroup
{
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int TotalQuantity { get; set; }
    public int Count { get; set; }
    public List<Guid> ReservationsIds { get; set; } = new();

}