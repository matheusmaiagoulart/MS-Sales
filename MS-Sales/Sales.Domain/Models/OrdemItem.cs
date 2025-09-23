namespace Sales.Domain.Models
{
    public class OrdemItem
    {
        public Guid IdProduct { get; init; }
        public int Quantity { get; init; }
        public OrdemItem(Guid idProduct, int quantity)
        {
            IdProduct = idProduct;
            Quantity = quantity;
        }
    }
}
