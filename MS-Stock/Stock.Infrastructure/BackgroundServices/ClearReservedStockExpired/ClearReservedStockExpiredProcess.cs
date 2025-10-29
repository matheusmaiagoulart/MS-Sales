using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Stock.Application.Interfaces;

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
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("service running on: {time}", DateTime.Now);
            await ClearExpiredReservations();
            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        }

        _logger.LogInformation("Service is stopping at: {time}", DateTime.Now);
    }


    private async Task ClearExpiredReservations()
    {
        using var scope = _scopeFactory.CreateScope();
        var productRepository = scope.ServiceProvider.GetRequiredService<IProductRepository>();
        var result = await productRepository.GetProductsExpiredWithStatusReserved();

        foreach (var item in result)
        {
            foreach (var id in item.ReservationsIds)
            {
                Console.WriteLine(
                    $"Canceling expired reservation. IdOrder: {id} - IdProduct: {item.ProductId} - Quantity: {item.TotalQuantity}");
                await productRepository.CancelReservation(id, item.ProductId, item.TotalQuantity);
            }
        }
    }
}