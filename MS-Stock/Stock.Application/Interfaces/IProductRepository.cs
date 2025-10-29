using Stock.Application.DTOs;

namespace Stock.Application.Interfaces;

public interface IProductRepository
{
    Task CreateProduct(Domain.Models.Product product);
    Task<bool> TryReserve(Guid idOrder, Guid idProduct, int quantity);
    Task<bool> ConfirmReservation (Guid idOrder, Guid idProduct, int quantity);
    Task<bool> CancelReservation (Guid idOrder, Guid idProduct, int quantity);
    void UpdateProduct(Domain.Models.Product product);
    Task<Domain.Models.Product?> GetProductById(Guid productId);
    Task<Domain.Models.Product?> GetProductByIdAsNoTracking(Guid productId);
    Task<IEnumerable<Domain.Models.Product?>> GetAllProductsAsNoTracking();
    Task<List<ProductStockExpiredGroup>> GetProductsExpiredWithStatusReserved();
    Task<decimal> GetProductPriceIfStockAvailable(Guid productId, int quantity);
    Task SaveChangesAsync();
}