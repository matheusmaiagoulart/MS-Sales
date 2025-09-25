using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Stock.Application.Products.Queries.StockValidation;
using Stock.Infrastructure.RabbitMQ.Producers;

namespace Stock.Infrastructure.RabbitMQ.Consumers;

public class ValidationStockAvailableConsumer
{
    private readonly StockValidationHandler _stockValidationHandler;

    public ValidationStockAvailableConsumer(StockValidationHandler stockValidationHandler)
    {
        _stockValidationHandler = stockValidationHandler;
    }


    public async Task<String> Consumer<T>()
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

                var items = JsonSerializer.Deserialize<StockValidationQuery>(message); 
                var result =  await _stockValidationHandler.Handle(items, CancellationToken.None);

                channel.BasicAckAsync(ea.DeliveryTag, false); 
                if(result.IsFailed)
                {
                    Console.WriteLine("Stock is not available for one or more items.");
                    return;
                }
                if(result.IsSuccess)
                {
                        Console.WriteLine("Resultado: " + result.Value.TotalAmount + " Disponível: " + result.Value.Available);
                }
                
                var publisherReponse = new ValidationStockAvailablePublisher();
                var messageReturnValidationStock = new
                {
                    //IdSale = result.Value.IdSale,
                    TotalAmount = result.Value.TotalAmount,
                    Available = result.Value.Available
                };
                
                publisher
                
            };

            var result = await channel.BasicConsumeAsync(
                queue: "orderValidationStockQueue",
                autoAck: false,
                consumer: consumer
            );
            
            return "Consumidor iniciado. Pressione [enter] para sair.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao conectar: {ex.Message}");
        }
        
        return "Erro ao iniciar o consumidor.";
    }
}

