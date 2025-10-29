using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sales.Application.Services;

namespace Sales.Infrastructure.RabbitMQ.Consumer;

public class GenericConsumer : IGenericConsumer
{
    public async Task Consumer<T>(string queueName, Func<T, Task> onMessag)
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            Port = 5672,
            UserName = "guest",
            Password = "guest",
            RequestedConnectionTimeout = TimeSpan.FromSeconds(20)
        };

        try
        {
            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();
            
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var obj = JsonSerializer.Deserialize<T>(message);
                
                await onMessag(obj);
                await channel.BasicAckAsync(ea.DeliveryTag, false);
                
            };
            
            var result = await channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao consumir a fila {queueName}: {ex.Message}");
            throw;
        }
    }
}