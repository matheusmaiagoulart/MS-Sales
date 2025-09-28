using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sales.Infrastructure.RabbitMQConfig.DTOs;

namespace Sales.Infrastructure.RabbitMQConfig.Consumer;

public class ConfigRabbitMQConsumer
{
    public async Task<RequestCreateOrderValidationResponse> Consume(string queue, Guid idOrder)
    {
        var cst = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var tcs = new TaskCompletionSource<RequestCreateOrderValidationResponse>();
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

                if (resultRequest.IdOrder.ToString() == idOrder.ToString())
                {
                    tcs.SetResult(resultRequest);
                    await channel.BasicAckAsync(ea.DeliveryTag, false); // Ack - Confirmacao de recebimento da mensagem e o processamento
                }
                else
                {
                    await channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
            };
            
            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

            // Chamada para iniciar consumo da fila e chamando consumer (validações acima)
            await channel.BasicConsumeAsync(queue: queue,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cst.Token);

            return await tcs.Task;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
    }
}