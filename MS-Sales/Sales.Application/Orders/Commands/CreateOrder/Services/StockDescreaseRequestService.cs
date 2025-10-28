using FluentResults;
using Microsoft.Extensions.Logging;
using Sales.Application.Services;
using Sales.Application.Utils;
using Sales.Domain.DTOs;

namespace Sales.Application.Orders.Commands.CreateOrder.Requests;

public class StockDescreaseRequestService : IStockDecreaseRequest
{
    private readonly IGenericPublisher _genericPublisher;
    private readonly IDecreaseStockResponseConsumer _consumerDecreaseStockResponse;
    private readonly ILogger<StockDescreaseRequestService> _logger;
    public StockDescreaseRequestService(
        IGenericPublisher genericPublisher,
        IDecreaseStockResponseConsumer consumerDecreaseStockResponse,
        ILogger<StockDescreaseRequestService> logger)
    {
        _genericPublisher = genericPublisher;
        _consumerDecreaseStockResponse = consumerDecreaseStockResponse;
        _logger = logger;
    }
    public async Task<Result<UpdateStockResponse>> SendDecreaseStockRequest(CreateOrderCommand request, Guid idOrder)
    {
        // Send message to decrease stock
        await _genericPublisher.Publish(request, QueuesRabbitMQ.REQUEST_DECREASE_STOCK, QueuesRabbitMQ.RESPONSE_DECREASE_STOCK, idOrder);
        _logger.LogInformation("POST api/Sales/Order - Stock validation successful, proceeding to decrease stock for Order ID: {IdSale}. Timestamp ({timestamp})", idOrder, CommonData.GetTimestamp());
        
        var resultDecreaseStock = await _consumerDecreaseStockResponse.Consume(QueuesRabbitMQ.RESPONSE_DECREASE_STOCK, idOrder, 30);
        if (resultDecreaseStock.IsFailed)
        {
            _logger.LogError("POST api/Sales/Order - Stock decreased request failed. Timestamp ({timestamp})", CommonData.GetTimestamp());
            return Result.Fail<UpdateStockResponse>(resultDecreaseStock.Errors);
        }
        if (!resultDecreaseStock.Value.IsSaleSuccess || resultDecreaseStock.ToResult().IsFailed)
        {
            _logger.LogError("POST api/Sales/Order - Stock decreased request failed. Timestamp ({timestamp})", CommonData.GetTimestamp());
            return Result.Fail<UpdateStockResponse>("An error occurred while decreasing stock.");
        }

        return resultDecreaseStock;
    }
}