using System.Collections.Concurrent;
using Sales.Application.Services;
using Sales.Domain.DTOs;

namespace Sales.Infrastructure.RabbitMQ.Consumer;

public class DecreaseStockResponseConsumer : IDecreaseStockResponseConsumer
{
    private readonly ConcurrentDictionary<Guid, UpdateStockResponse> _responsesList = new();
    
    public async Task<UpdateStockResponse> Consume(string queue, Guid idOrder, int tokenSourceTimeout)
    {
        try
        {

        
        // Verify if the response already exists in the dictionary
        var existsResponse = GetResponseByIdOrder(idOrder);
        if (existsResponse != null)
            return existsResponse;
        
        // Set timeout for waiting the response in the queue
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(tokenSourceTimeout)); 
        
        // Use TaskCompletionSource to manage the async task and return the value if completed, or null if it times out
        var tcs = new TaskCompletionSource<UpdateStockResponse>(); 
        
        // Set manually the value if the timeout is reached
        cts.Token.Register(() => tcs.TrySetResult(null)); 
        
        var genericConsumer = new GenericConsumer();
        await genericConsumer.Consumer<UpdateStockResponse>(queue, async (result) =>
        {

            _responsesList.TryAdd(result.IdOrder, result);
            Console.WriteLine("Mensagem recebida da fila: " + result);
            var responseByIdOrder = GetResponseByIdOrder(idOrder);
            if (responseByIdOrder != null)
            {
                tcs.SetResult(responseByIdOrder); // Retorna a resposta para o chamador
            }

        }); 
        return await tcs.Task;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private UpdateStockResponse? GetResponseByIdOrder(Guid idOrder)
    {
        if (_responsesList.TryRemove(idOrder, out var response))
            return response;
        
        return null;
    }
}