using System.Text.Json;
using FluentResults;
using RabbitMQ.Client;
using Stock.Application.Products.Queries.StockValidation;
using Stock.Infrastructure.RabbitMQ.Producers;

namespace Stock.Infrastructure.RabbitMQ.Consumers;

public class ValidationStockAvailableConsumer
{
    private readonly StockValidationHandler _stockValidationHandler;
    private readonly GenericConsumer _genericConsumer;
    const string queueResponse = "orderResponseValidationStockQueue";

    public ValidationStockAvailableConsumer(StockValidationHandler stockValidationHandler,
        GenericConsumer genericConsumer)
    {
        _stockValidationHandler = stockValidationHandler;
        _genericConsumer = genericConsumer;
    }


    public async Task Consumer<T>(string queueName)
    {
        var consumerQueue = new GenericConsumer();
        await consumerQueue.Consumer<StockValidationQuery>(queueName, async (result) =>
        {
            // Chama o handler para validar o estoque
            var resultValidation = await _stockValidationHandler.Handle(result, CancellationToken.None);
            
            var responseFinal = new StockValidationResponse(result.IdOrder, false, 0);
            if (resultValidation.IsSuccess)
            {
                Console.WriteLine("Estoque disponível: " + resultValidation.Value.Available);
                responseFinal = resultValidation.Value;
            }
            else
            {
                Console.WriteLine("Estoque indisponível para o pedido: " + responseFinal.IdOrder);
            }

            var responseProps = new BasicProperties() { ReplyTo = queueResponse };

           await ReturnResponseStockValidation(responseFinal, responseProps);
        });
    }


    public async Task ReturnResponseStockValidation(StockValidationResponse response, BasicProperties basicProperties)
    {
        var publisherReponse = new ValidationStockAvailablePublisher(); //Instanciando a classe de publicacao
        var messageReturnValidationStock =
            new StockValidationResponse(response.IdOrder, response.Available,
                response.TotalAmount); //Adicionando os dados de retorno
        Console.WriteLine(JsonSerializer.Serialize(messageReturnValidationStock) + " Mensagem de retorno");
        await publisherReponse.Publisher(messageReturnValidationStock, basicProperties);
    }
}