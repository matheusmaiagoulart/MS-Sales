using System.Text.Json;
using RabbitMQ.Client;

namespace Sales.Infrastructure.RabbitMQConfig;

public class RabbitMQPublisher
{

    private readonly ConfigRabbitMQ _configRabbitMq;

    public RabbitMQPublisher(ConfigRabbitMQ configRabbitMq)
    {
        _configRabbitMq = configRabbitMq;
    }

    public async Task Publish<T>(T order, string queue)
    {
        var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };
        
         var connection = await factory.CreateConnectionAsync();
         var channel = await connection.CreateChannelAsync();
        
        await channel.QueueDeclareAsync(queue: queue,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var message = JsonSerializer.Serialize(order);
        var body = System.Text.Encoding.UTF8.GetBytes(message);
        
        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: queue,
            mandatory: false,
            body: body,
            cancellationToken: CancellationToken.None);
        
        EventoConfirmacao<T>(order);
    }
    
   

    public void EventoConfirmacao<T>(T order)
    {
        Console.WriteLine("Mensagem enviada para a fila RabbitMQ." + JsonSerializer.Serialize<T>(orderR));
    }
    

}