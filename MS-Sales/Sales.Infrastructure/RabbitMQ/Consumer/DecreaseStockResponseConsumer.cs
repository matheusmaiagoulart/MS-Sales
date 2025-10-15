using System.Collections.Concurrent;
using FluentResults;
using Sales.Application.Services;
using Sales.Application.Utils;
using Sales.Domain.DTOs;

namespace Sales.Infrastructure.RabbitMQ.Consumer;

public class DecreaseStockResponseConsumer : IDecreaseStockResponseConsumer
{
    private readonly ConcurrentDictionary<Guid, UpdateStockResponse> _responsesList = new();
    private readonly TaskCompletionSource<UpdateStockResponse> _tcs = new();
    private readonly IGenericConsumer _genericConsumer;
    public DecreaseStockResponseConsumer(IGenericConsumer genericConsumer)
    {
        _genericConsumer = genericConsumer;

        _genericConsumer.Consumer<UpdateStockResponse>(
            QueuesRabbitMQ.RESPONSE_DECREASE_STOCK, async (result) =>
            {
                Console.WriteLine($"Mensagem recebida: {result.IdOrder}");
                _responsesList[result.IdOrder] = result; //Add or Update if key exists
            }
        );
    }

    public async Task<Result<UpdateStockResponse>> Consume(string queue, Guid idOrder, int timeoutSeconds)
    {

        var responseDictonary = GetResponseInDictionary(idOrder);
        if (responseDictonary != null)
            return responseDictonary;
        
        try
        {
            var maxRetry = timeoutSeconds;
            var timeout = 1; // seconds
            
            
            for (int i = 0; i < maxRetry; i++)
            {
                var response = GetResponseInDictionary(idOrder);
                if (response != null)
                {
                    return Result.Ok(response);
                }

                if (i < maxRetry - 1)
                    await Task.Delay(TimeSpan.FromSeconds(timeout));
                
            }
        }
        catch (OperationCanceledException e)
        {
            return Result.Fail<UpdateStockResponse>("Operation canceled: " + e.Message);
        }
        
        return Result.Fail<UpdateStockResponse>("Timeout: No response received within the specified time." );
    }


    private UpdateStockResponse? GetResponseInDictionary(Guid idOrder)
        {
            if (_responsesList.TryRemove(idOrder, out var response))
                return response;

            return null;
        }
    
}