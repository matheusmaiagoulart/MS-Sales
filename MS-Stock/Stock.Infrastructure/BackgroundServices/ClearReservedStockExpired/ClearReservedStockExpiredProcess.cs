using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stock.Domain.Models.Interfaces;

namespace Stock.Infrastructure.BackgroundServices.ClearReservedStockExpired;

public class ClearReservedStockExpiredProcess : BackgroundService
{
    private readonly ILogger<ClearReservedStockExpiredProcess> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    public ClearReservedStockExpiredProcess(ILogger<ClearReservedStockExpiredProcess> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        
        while(!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Serviço em execução em: {time}", DateTime.Now);
            
            await ClearExpiredReservations();
            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        }
        _logger.LogInformation("Serviço parado.");
    }


private async Task ClearExpiredReservations()
{
    using var scope = _scopeFactory.CreateScope();
    var _productRepository = scope.ServiceProvider.GetRequiredService<IProductRepository>();
    var result = await _productRepository.GetProductsExpiredWithStatusReserved();
    
    foreach (var item in result)
    {
        foreach (var id in item.ReservationsIds)
        {
            Console.WriteLine($"Cancelando reserva expirada. IdOrder: {id} - IdProduct: {item.ProductId} - Quantity: {item.TotalQuantity}");
            await _productRepository.CancelReservation(id, item.ProductId, item.TotalQuantity);
        }
        
    }
}

}