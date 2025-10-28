using System.Runtime.InteropServices.JavaScript;

namespace Stock.Domain.Models;

public class StockReservation
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime ReservedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public ReservationStatus Status { get; set; }
    
    protected StockReservation() { }
    
    public StockReservation(Guid orderId, Guid productId, int quantity, ReservationStatus status)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        ReservedAt = DateTime.Now;
        ExpiresAt = ReservedAt.AddMinutes(2);
        Status = status;
    }
}

public enum ReservationStatus
{
    Reserved,
    Confirmed,
    Cancelled,
    Expired
}