using Microsoft.EntityFrameworkCore;
using Stock.Domain.Interfaces;
using Stock.Domain.Models;
using Stock.Infrastructure.Data.Context;

namespace Stock.Infrastructure.Repository;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateProduct(Product product)
    {
        await _context.Stock.AddAsync(product);
    }

    public async Task<bool> DecreaseStock(Guid idProduct, int quantity)
    {
        var result = await  _context.Stock
            .Where(x => x.IdProduct == idProduct && x.StockQuantity >= quantity)
            .ExecuteUpdateAsync(set => set
                .SetProperty(p => p.StockQuantity, p =>  p.StockQuantity - quantity)
                .SetProperty(p => p.UpdatedAt, p => DateTime.UtcNow)
            );

        return result > 0;
    }
    
    public void UpdateStock(Product product)
    {
        _context.Stock.Update(product);
    }


    public async Task<Product?> GetProductById(Guid productId)
    {
        return await _context.Stock
            .FirstOrDefaultAsync(p => p.IdProduct == productId);
    }

    public async Task<Product?> GetProductByIdAsNoTracking(Guid productId)
    {
        return await _context.Stock
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdProduct == productId);
    }

    public async Task<IEnumerable<Product?>> GetAllProductsAsNoTracking()
    {
        return await _context.Stock
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<decimal> GetProductPriceIfStockAvailable(Guid productId, int quantity)
    {
        var product =  await _context.Stock
            .FirstOrDefaultAsync(p => p.IdProduct == productId && p.StockQuantity >= quantity);
        if (product == null)
            return 0;
        
        return product.Price;
    }

public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    
    
}