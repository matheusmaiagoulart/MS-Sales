using System.Collections.Concurrent;
using System.Text.Json;
using Sales.Domain.DTOs;
using Sales.Infrastructure.RabbitMQ.ConfigConnection;

namespace Sales.Infrastructure.RabbitMQConfig.Consumer;

public class ConfigRabbitMQConsumer
{
    private readonly ConcurrentDictionary<Guid, RequestCreateOrderValidationResponse> _responsesList = new();

    public async Task<RequestCreateOrderValidationResponse> Consume(string queue, Guid idOrder, int tokenSourceTimeout)
    {
        var existsResponse = GetResponseByIdOrder(idOrder);
        if (existsResponse != null)
        {
            return existsResponse;
        }
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(tokenSourceTimeout)); // Tempo máximo de espera pela resposta na fila
        var tcs = new TaskCompletionSource<RequestCreateOrderValidationResponse>(); //Classe para gerenciar a tarefa assíncrona e retornar o valor caso seja concluída com sucesso, ou null se passar de 10 segundos
        
        cts.Token.Register(() => tcs.TrySetResult(null)); // Se o tempo esgotar, retorna null (Só consegue atribuir valor uma vez)
        
        var genericConsumer = new GenericConsumer();
        await genericConsumer.Consumer<RequestCreateOrderValidationResponse>(queue, async (result) =>
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

    private RequestCreateOrderValidationResponse? GetResponseByIdOrder(Guid idOrder)
    {
        if (_responsesList.TryRemove(idOrder, out var response))
            return response;
        
        return null;
    }
    

    
    
}