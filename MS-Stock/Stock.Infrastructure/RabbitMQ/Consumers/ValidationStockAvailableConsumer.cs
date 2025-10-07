using System.Text.Json;
using FluentResults;
using MediatR;
using RabbitMQ.Client;
using Stock.Application.Products.Commands.UpdateStock;
using Stock.Application.Products.Queries.StockValidation;
using Stock.Infrastructure.RabbitMQ.Config;
using Stock.Infrastructure.RabbitMQ.Interfaces;
using Stock.Infrastructure.RabbitMQ.Producers;

namespace Stock.Infrastructure.RabbitMQ.Consumers;

public class ValidationStockAvailableConsumer
{
    private readonly IRequestHandler<StockValidationQuery, Result<StockValidationResponse>>
        _stockValidationHandler; //Interface do handler de validacao de estoque

    private readonly IGenericConsumer _genericConsumer;
    private readonly IGenericPublisher _genericPublisher;
    private const string queueResponse = QueuesConfig.RabbitMQQueues.RESPONSE_VALIDATION_STOCK;

    public ValidationStockAvailableConsumer(
        IRequestHandler<StockValidationQuery, Result<StockValidationResponse>> stockValidationHandler,
        IGenericConsumer genericConsumer, IGenericPublisher genericPublisher)
    {
        _stockValidationHandler = stockValidationHandler;
        _genericConsumer = genericConsumer;
        _genericPublisher = genericPublisher;
    }


    public async Task Consumer<T>(string queueName)
    {
        await _genericConsumer.Consumer<StockValidationQuery>(queueName, async (result) =>
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
        var messageReturnValidationStock = new StockValidationResponse
        (
            response.IdOrder,
            response.Available,
            response.TotalAmount
        );
        Console.WriteLine(JsonSerializer.Serialize(messageReturnValidationStock) + " Mensagem de retorno");
        await _genericPublisher.Publisher(messageReturnValidationStock, basicProperties);
    }
}