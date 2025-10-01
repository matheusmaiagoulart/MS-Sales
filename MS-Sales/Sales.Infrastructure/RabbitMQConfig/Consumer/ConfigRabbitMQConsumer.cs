using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sales.Infrastructure.RabbitMQConfig.DTOs;

namespace Sales.Infrastructure.RabbitMQConfig.Consumer;

public class ConfigRabbitMQConsumer
{
    private readonly ConcurrentDictionary<Guid, RequestCreateOrderValidationResponse> _responsesList = new();
    public async Task<RequestCreateOrderValidationResponse> Consume(string queue, Guid idOrder)
    {
        var getResponseBeforeConsume = GetResponseByIdOrder(idOrder);
        if (getResponseBeforeConsume != null)
            return getResponseBeforeConsume;
        
        
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)); // Tempo máximo de espera pela resposta na fila
        var tcs = new TaskCompletionSource<RequestCreateOrderValidationResponse>();//Classe para gerenciar a tarefa assíncrona e retornar o valor caso seja concluída com sucesso, ou null se passar de 10 segundos
        
        cts.Token.Register(() => tcs.TrySetResult(null)); // Se o tempo esgotar, retorna null (Só consegue atribuir valor uma vez)
        
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest"
        };

        try
        {
            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            // Validacões para consumo da fila
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = System.Text.Encoding.UTF8.GetString(body);
                var resultRequest =
                    JsonSerializer.Deserialize<RequestCreateOrderValidationResponse>(message); // Transforma pra DTO
                _responsesList.TryAdd(resultRequest.IdOrder, resultRequest);
                var responseByIdOrder = GetResponseByIdOrder(idOrder);
                if (responseByIdOrder != null)
                {
                    tcs.SetResult(responseByIdOrder); // Retorna a resposta para o chamador
                }
                await channel.BasicAckAsync(ea.DeliveryTag, false);
            };

            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

            // Chamada para iniciar consumo da fila e chamando consumer (validações acima)
            await channel.BasicConsumeAsync(queue: queue,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cts.Token);

            return await tcs.Task;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }

    private RequestCreateOrderValidationResponse? GetResponseByIdOrder(Guid idOrder)
    {
        if (_responsesList.TryRemove(idOrder, out var response))
            return response;
        
        return null;
    }
    

    
    
}