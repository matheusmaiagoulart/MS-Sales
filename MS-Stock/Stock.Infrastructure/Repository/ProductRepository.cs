using Microsoft.EntityFrameworkCore;
using Microsoft.Win32.SafeHandles;
using Stock.Application.DTOs;
using Stock.Domain.Models;
using Stock.Domain.Models.Interfaces;
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

    public async Task<bool> TryReserve(Guid idOder, Guid idProduct, int quantity)
    {
        var result = await  _context.Stock
            .Where(x => x.IdProduct == idProduct && x.StockQuantity >= quantity)
            .ExecuteUpdateAsync(set => set
                .SetProperty(p => p.StockQuantity, p =>  p.StockQuantity - quantity)
                .SetProperty(p => p.ReservedQuantity, p => p.ReservedQuantity + quantity)
                .SetProperty(p => p.UpdatedAt, p => DateTime.UtcNow)
            );


        if (result > 0)
        {
            await _context.StockReservations.AddAsync(new StockReservation(idOder, idProduct, quantity,
                ReservationStatus.Reserved));
            await _context.SaveChangesAsync();

            return true;
        }
        return false;
    }

    public async Task<bool> ConfirmReservation(Guid idOrder, Guid idProduct, int quantity)
    {
        var result = await _context.Stock
            .Where(x => x.IdProduct == idProduct && x.ReservedQuantity >= quantity)
            .ExecuteUpdateAsync(set => set
                .SetProperty(p => p.ReservedQuantity, p => p.ReservedQuantity - quantity));
        
        if (result > 0)
        {
            await _context.StockReservations.Where(x => x.OrderId == idOrder).ExecuteUpdateAsync(set => set
                .SetProperty(x => x.Status, x => ReservationStatus.Confirmed));
            return true;
        }
        return false;
    }

    public async Task<bool> CancelReservation(Guid idOrder, Guid idProduct, int quantity)
    {
        var result = await _context.Stock
            .Where(x => x.IdProduct == idProduct && x.ReservedQuantity >= quantity)
            .ExecuteUpdateAsync(set => set
                .SetProperty(p => p.StockQuantity, p => p.StockQuantity + quantity)
                .SetProperty(p => p.ReservedQuantity, p => p.ReservedQuantity - quantity)
            );
        
        if(result > 0)
        {
            await _context.StockReservations.Where(x => x.OrderId == idOrder).ExecuteUpdateAsync(set => set
                .SetProperty(x => x.Status, x => ReservationStatus.Expired));
        }
        
        return result > 0;
    }

    public async Task<List<ProductStockExpiredGroup>> GetProductsExpiredWithStatusReserved()
    {
        return await _context.StockReservations
            .Where(x => x.ExpiresAt < DateTime.Now  && x.Status == ReservationStatus.Reserved)
            .GroupBy(x => x.ProductId)
            .Select(x => new ProductStockExpiredGroup
            {
                ProductId = x.Key,
                TotalQuantity = x.Sum(y => y.Quantity),
                Count = x.Count(),
                ReservationsIds = x.Select(x => x.OrderId).ToList()
            }).ToListAsync();
    }
    
    
    public void UpdateProduct(Product product)
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