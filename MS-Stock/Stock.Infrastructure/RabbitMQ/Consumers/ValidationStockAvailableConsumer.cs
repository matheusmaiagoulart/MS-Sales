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
            var response = new StockValidationResponse(Guid.Empty, false, 0);
            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var items = JsonSerializer.Deserialize<StockValidationQuery>(message);
                var result = await _stockValidationHandler.Handle(items, CancellationToken.None); // Chama o handler para validar o estoque

                var correlationId = ea.BasicProperties.CorrelationId;
                var replyTo = ea.BasicProperties.ReplyTo;
                var responseProps = new BasicProperties()
                {
                    CorrelationId = correlationId,
                    ReplyTo = replyTo
                };
                
                channel.BasicAckAsync(ea.DeliveryTag, false);
                
                if (result.IsFailed)
                {
                    Console.WriteLine("Falha na validação de estoque para o pedido: " + items.IdOrder);
                    // Cria resposta de falha
                     response = new StockValidationResponse(items.IdOrder, false, 0);
                }
                else
                {
                    Console.WriteLine("Resultado: " + result.Value.TotalAmount + " Disponível: " +
                                      result.Value.Available + " IdOrder: " + result.Value.IdOrder);
                    response = result.Value;
                } 
                await ReturnResponseStockValidation(response, responseProps);
                Console.WriteLine(response + "dskfjs");
            };

            var result = await channel.BasicConsumeAsync(
                queue: "orderValidationStockQueue",
                autoAck: false,
                consumer: consumer
            );
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao conectar: {ex.Message}");
        }

        return "Erro ao iniciar o consumidor.";
    }

    public async Task ReturnResponseStockValidation(StockValidationResponse response, BasicProperties basicProperties)
    {
        var publisherReponse = new ValidationStockAvailablePublisher(); //Instanciando a classe de publicacao
        var messageReturnValidationStock = new StockValidationResponse(response.IdOrder, response.Available, response.TotalAmount); //Adicionando os dados de retorno
        Console.WriteLine(JsonSerializer.Serialize(messageReturnValidationStock) + " Mensagem de retorno");
        await publisherReponse.Publisher(messageReturnValidationStock, basicProperties);
    }
}