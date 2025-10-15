using System.Text.Json;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Stock.Application.Products.Commands.UpdateStock;
using Stock.Infrastructure.RabbitMQ.Interfaces;

namespace Stock.Infrastructure.RabbitMQ.Consumers;

public class DecreaseStockConsumer
{
    private readonly IGenericConsumer _genericConsumer;
    private readonly IGenericPublisher _genericPublisher;
    private readonly IRequestHandler<UpdateStockCommand, Result<UpdateStockCommandResponse>> _handler;
    private readonly ILogger<DecreaseStockConsumer> _logger;

    public DecreaseStockConsumer(IGenericConsumer genericConsumer,
        IRequestHandler<UpdateStockCommand, Result<UpdateStockCommandResponse>> handler,
        IGenericPublisher genericPublisher, ILogger<DecreaseStockConsumer> logger)
    {
        _handler = handler;
        _genericConsumer = genericConsumer;
        _genericPublisher = genericPublisher;
        _logger = logger;
    }


    public async Task Consumer<T>(string queueName, string queueResponse)
    {
        await _genericConsumer.Consumer<UpdateStockCommand>(queueName, async (message) =>
        {
            var response = new UpdateStockCommandResponse(message.IdOrder, false);
            var basicProperties = new BasicProperties() { ReplyTo = queueResponse };
            _logger.LogInformation("Message received to decrease stock: " + message.Items.Count + " items.");
            var result = await _handler.Handle(message, CancellationToken.None);

            if (result.IsFailed)
            {
                _logger.LogError("Error decreasing stock for order: " + message.IdOrder);
                await ReturnResponseDecreaseStock(response, basicProperties);
            }
            else
            {
                _logger.LogInformation("Stock successfully decreased for order: " + message.IdOrder);
                response.IsSaleSuccess = true;
                await ReturnResponseDecreaseStock(response, basicProperties);
            }
        });
    }

    private async Task ReturnResponseDecreaseStock(UpdateStockCommandResponse response, BasicProperties basicProperties)
    {
        _genericPublisher.Publisher(response, basicProperties);
        _logger.LogInformation("Sent response for stock decrease for order: " + response.IdOrder);
    }
}