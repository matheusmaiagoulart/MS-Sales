using FluentResults;
using Microsoft.Extensions.Logging;
using Sales.Application.Services;
using Sales.Application.Utils;
using Sales.Domain.DTOs;

namespace Sales.Application.Orders.Commands.CreateOrder.Requests;

public class StockValidationRequestService : IStockValidationRequest
{
    private readonly IGenericPublisher _genericPublisher;
    private readonly IValidationStockResponseConsumer _consumerStockValidationResponse;
    private readonly ILogger<StockValidationRequestService> _logger;
    public StockValidationRequestService(
        IGenericPublisher genericPublisher,
        IValidationStockResponseConsumer consumerStockValidationResponse,
        ILogger<StockValidationRequestService> logger)
    {
        _genericPublisher = genericPublisher;
        _consumerStockValidationResponse = consumerStockValidationResponse;
        _logger = logger;
    }
    public async Task<Result<RequestCreateOrderValidationResponse>> SendStockValidationRequest(CreateOrderCommand request, Guid idOrder)
    {
        // Send message to validate stock
        await _genericPublisher.Publish(request, QueuesRabbitMQ.REQUEST_VALIDATION_STOCK, QueuesRabbitMQ.RESPONSE_VALIDATION_STOCK, idOrder);
        _logger.LogInformation("POST api/Sales/Order - Validated order request, proceeding to stock validation for Order ID: {IdSale}. Timestamp ({timestamp})", idOrder, CommonData.GetTimestamp());
        
        var responseStockValidation= await _consumerStockValidationResponse.Consume(QueuesRabbitMQ.RESPONSE_VALIDATION_STOCK, idOrder, 30);
        if(responseStockValidation.IsFailed)
            return Result.Fail<RequestCreateOrderValidationResponse>(responseStockValidation.Errors);
        if (!responseStockValidation.Value.IsStockAvailable)
            return Result.Fail<RequestCreateOrderValidationResponse>("Stock is not available for one or more products in the order.");
        
        return responseStockValidation;
    }
}