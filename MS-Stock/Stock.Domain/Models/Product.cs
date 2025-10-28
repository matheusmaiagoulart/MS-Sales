using System.ComponentModel.DataAnnotations;

namespace Stock.Domain.Models
{
    public class Product
    {
        [Key] public Guid IdProduct { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public decimal Price { get; private set; }
        public int StockQuantity { get; private set; }
        
        public int ReservedQuantity { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }


        protected Product()
        {
        }

        public Product(string name, string description, decimal price, int stockQuantity)
        {
            IdProduct = Guid.NewGuid();
            Name = name;
            Description = description;
            Price = price;
            StockQuantity = stockQuantity;
            CreatedAt = DateTime.Now;
            UpdatedAt = null;
            ReservedQuantity = 0;
        }

        public void UpdateProduct(string name, string description, decimal price)
        {
            Name = name;
            Description = description;
            Price = price;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}