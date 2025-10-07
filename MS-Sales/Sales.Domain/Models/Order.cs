using System.ComponentModel.DataAnnotations;

namespace Sales.Domain.Models
{
    public class Order
    {

        [Key]
        public Guid IdSale { get; private set; }
        public List<OrdemItem> OrdemItens { get; private set; } = new List<OrdemItem>();
        public decimal TotalAmount { get; private set; }
        public StatusSale Status { get; private set; }
        
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }


        protected Order() { }

        public Order(List<OrdemItem> ordemItems, decimal totalAmount)
        {
            IdSale = Guid.NewGuid();
            OrdemItens = ordemItems;
            TotalAmount = totalAmount;
            Status = StatusSale.PENDING;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = null;
        }
        public Order(Guid idSale, List<OrdemItem> ordemItems, decimal totalAmount)
        {
            IdSale = idSale;
            OrdemItens = ordemItems;
            TotalAmount = totalAmount;
            Status = StatusSale.PENDING;
            CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"));
            UpdatedAt = null;
        }
        
        public void UpdateStatusOrder(StatusSale newStatus)
        {
            Status = newStatus;
            UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"));
        }
        
    }
    public enum StatusSale
    {
        PENDING,
        COMPLETED,
        CANCELED
    }
    
}
