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
        public Order(Guid idSale, List<OrdemItem> ordemItems, decimal totalAmount, StatusSale status, DateTime createdAt, DateTime? updatedAt)
        {
            IdSale = idSale;
            OrdemItens = ordemItems;
            TotalAmount = totalAmount;
            Status = status;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }
        
        public void UpdateStatusOrder(StatusSale newStatus)
        {
            Status = newStatus;
            UpdatedAt = DateTime.UtcNow;
        }
        
    }
    public enum StatusSale
    {
        PENDING,
        COMPLETED,
        CANCELED
    }
    
}
