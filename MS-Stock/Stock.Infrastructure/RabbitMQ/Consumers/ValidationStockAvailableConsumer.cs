using System.Text.Json;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Stock.Application.Product.Queries.StockValidation;
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
    private readonly ILogger<ValidationStockAvailableConsumer> _logger;

    public ValidationStockAvailableConsumer(
        IRequestHandler<StockValidationQuery, Result<StockValidationResponse>> stockValidationHandler,
        IGenericConsumer genericConsumer, IGenericPublisher genericPublisher, ILogger<ValidationStockAvailableConsumer> logger)
    {
        _stockValidationHandler = stockValidationHandler;
        _genericConsumer = genericConsumer;
        _genericPublisher = genericPublisher;
        _logger = logger;
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
                _logger.LogInformation("Stock available for order: " + resultValidation.Value.IdOrder);
                responseFinal = resultValidation.Value;
            }
            else
            {
                _logger.LogError("Stock not available for order: " + responseFinal.IdOrder);
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
        _logger.LogInformation("Sending stock validation response for order: " + response.IdOrder);
        await _genericPublisher.Publisher(messageReturnValidationStock, basicProperties);
    }
}