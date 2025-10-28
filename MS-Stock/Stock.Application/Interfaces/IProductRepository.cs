using Stock.Application.DTOs;

namespace Stock.Domain.Models.Interfaces;

public interface IProductRepository
{
    Task CreateProduct(Product product);
    Task<bool> TryReserve(Guid idOrder, Guid idProduct, int quantity);
    Task<bool> ConfirmReservation (Guid idOrder, Guid idProduct, int quantity);
    Task<bool> CancelReservation (Guid idOrder, Guid idProduct, int quantity);
    void UpdateProduct(Product product);
    Task<Product?> GetProductById(Guid productId);
    Task<Product?> GetProductByIdAsNoTracking(Guid productId);
    Task<IEnumerable<Product?>> GetAllProductsAsNoTracking();
    Task<List<ProductStockExpiredGroup>> GetProductsExpiredWithStatusReserved();
    Task<decimal> GetProductPriceIfStockAvailable(Guid productId, int quantity);
    Task SaveChangesAsync();
}