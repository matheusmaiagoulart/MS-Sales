using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using Sales.Application.Orders.Commands.CreateOrder;
using Sales.Application.Services;
using Sales.Application.Utils;

namespace Sales.Infrastructure.Services;

public class StockValidationService : IStockValidationService
{

    private readonly ILogger<StockValidationService> _logger;
    private readonly IValidationStockResponseConsumer _rabbitMQConsumer;
    
    public StockValidationService(
        ILogger<StockValidationService> logger,
        IValidationStockResponseConsumer rabbitMQConsumer)
    {
        _logger = logger;
        _rabbitMQConsumer = rabbitMQConsumer;
    }

    public async Task<Result<T>> ValidateStockAsync<T>(CreateOrderCommand request, Guid orderId, string queueName, CancellationToken cancellationToken)
    {
        try
        {
            var timestamp = CommonData.GetTimestamp();
            
            var maxRetry = 4;
            var timeout = 3; // Tempo em segundos para esperar a resposta
            
            object response = null;


            for (int i = 0; i < maxRetry; i++)
            {
                try
                {
                    // Tenta consumir a mensagem da fila, esperando no máximo "timeout" segundos

                    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeout)); // Definindo o tempo limite de espera da resposta
                    response = await Task.Run(() => _rabbitMQConsumer.Consume(queueName, orderId, timeout));

                    if (response != null)
                        break;

                }
                catch (OperationCanceledException e) // Exception lancada quando o tempo de espera acaba
                {
                    _logger.LogInformation(
                        "Attempt {Attempt} - No response received from stock validation service within {Timeout} seconds for Order ID: {IdSale}. Retrying... Timestamp ({timestamp})");
                    if (i == maxRetry - 1)
                        break;
                }

                await Task.Delay(2000, cancellationToken);

            }

            if (response == null)
            {
                _logger.LogError(
                    "POST api/Sales/Order - Timeout: No response from stock validation service after multiple attempts for Order ID: {IdSale}. Timestamp ({timestamp})",
                    orderId, timestamp);

                return Result.Fail<T>("Timeout: No response from stock validation service.");
            }
            return Result.Ok((T)response);
            
            // acaba aqui e retorna o response

            // if (!responseStock.IsStockAvailable)
            // {
            //     Console.WriteLine(JsonSerializer.Serialize(responseStock));
            //     _logger.LogWarning("POST api/Sales/Order - Stock is not available for one or more items. " + "\n" +
            //                        "The order ID: {IdSale}. Timestamp ({timestamp})", orderId, timestamp);
            //     return Result.Fail<T>("Stock is not available for one or more items.");
            // }
            // return Result.Ok(responseStock);
        }
        catch (OperationCanceledException e)
        {
            Console.WriteLine(e);
            throw;
        }

    }
}