using System.Text.Json;
using RabbitMQ.Client;
using Sales.Application.Services;

namespace Sales.Infrastructure.RabbitMQ.Producer;

public class GenericProducer : IGenericPublisher
{

    public async Task Publish<T>(T order, string queuePub, string? queueResponse, Guid idOrder)
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
         
         var props = new BasicProperties()
         {
             ReplyTo = queueResponse
         };
        
        await channel.QueueDeclareAsync(queue: queuePub,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var message = JsonSerializer.Serialize(order);
        var body = System.Text.Encoding.UTF8.GetBytes(message);
        
        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: queuePub,
            basicProperties: props,
            mandatory: false,
            body: body,
            cancellationToken: CancellationToken.None);
    }
    
    

}