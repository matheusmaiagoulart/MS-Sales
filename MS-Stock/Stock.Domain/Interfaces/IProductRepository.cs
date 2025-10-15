using Stock.Domain.Models;

namespace Stock.Domain.Interfaces;

public interface IProductRepository
{
    Task CreateProduct(Product product);
    Task<bool> DecreaseStock(Guid idProduct, int quantity);
    void UpdateProduct(Product product);
    Task<Product?> GetProductById(Guid productId);
    Task<Product?> GetProductByIdAsNoTracking(Guid productId);
    Task<IEnumerable<Product?>> GetAllProductsAsNoTracking();
    
    Task<decimal> GetProductPriceIfStockAvailable(Guid productId, int quantity);
    Task SaveChangesAsync();
}