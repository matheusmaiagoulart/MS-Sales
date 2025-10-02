using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Stock.Infrastructure.RabbitMQ.Consumers;

public class GenericConsumer
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
                Console.WriteLine($"Mensagem recebida na fila {queueName}: {message}");
                
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
            Console.WriteLine($"Error to consume queue {queueName}: {ex.Message}");
            throw;
        }
        
    }
}

public class ConsumerResult<T>
{
    public string Mensagem { get; set; }
    public T Objeto { get; set; }
}