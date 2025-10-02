using System.Text.Json;
using RabbitMQ.Client;

namespace Stock.Infrastructure.RabbitMQ.Producers;

public class ValidationStockAvailablePublisher
{
    public async Task Publisher<T>(T messageReturnValidationStock, BasicProperties basicProperties)
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
            
            await channel.QueueDeclareAsync(queue: basicProperties.ReplyTo,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            
            var message = JsonSerializer.Serialize(messageReturnValidationStock);
            var body = System.Text.Encoding.UTF8.GetBytes(message);
            
            var correlationId = basicProperties.CorrelationId;
            
            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: basicProperties.ReplyTo,
                mandatory: false,
                basicProperties: basicProperties, 
                body: body,
                cancellationToken: CancellationToken.None);
                
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
        }
        
        
    }
}