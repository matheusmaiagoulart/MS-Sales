using System.Text.Json;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Sales.Application.Services;
using Sales.Application.Utils;
using Sales.Domain.Interfaces;
using Sales.Domain.Models;

namespace Sales.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<CreateOrderResponse>>
{
    private readonly IValidator<CreateOrderCommand> _validator;
    private readonly IGenericPublisher _genericPublisher;
    private readonly ILogger<CreateOrderCommandHandler> _logger;
    private readonly IDecreaseStockResponseConsumer _consumerDecreaseStockResponse;
    private readonly IValidationStockResponseConsumer _consumerStockValidationResponse;
    private readonly IOrderRepository _orderRepository;

    public CreateOrderCommandHandler
    (
        ILogger<CreateOrderCommandHandler> logger,
        IValidator<CreateOrderCommand> validator,
        IOrderRepository orderRepository,
        IGenericPublisher genericPublisher,
        IValidationStockResponseConsumer consumerStockValidationResponse,
        IDecreaseStockResponseConsumer consumerDecreaseStockResponse
    )
    {
        _logger = logger;
        _validator = validator;
        _orderRepository = orderRepository;
        _genericPublisher = genericPublisher;
        _consumerDecreaseStockResponse = consumerDecreaseStockResponse;
        _consumerStockValidationResponse = consumerStockValidationResponse;
    }

    
    public async Task<Result<CreateOrderResponse>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var timestamp = CommonData.GetTimestamp();
        _logger.LogInformation("POST api/Sales/Order - Starting order creation process. Timestamp ({timestamp})", timestamp);

        // FluentResult Validation
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogError("POST api/Sales/Order - Validation of initial attributes failed. Timestamp ({timestamp})", timestamp);
            var errors = validationResult.Errors
                .Select(e => new Error(e.ErrorMessage));

            return Result.Fail<CreateOrderResponse>(errors);
        }

        var IdSale = Guid.NewGuid();
        request.setIdOrder(IdSale);

        // Send message to validate stock
        await _genericPublisher.Publish(request, QueuesRabbitMQ.REQUEST_VALIDATION_STOCK, QueuesRabbitMQ.RESPONSE_VALIDATION_STOCK, IdSale);
        _logger.LogInformation("POST api/Sales/Order - Validated order request, proceeding to stock validation for Order ID: {IdSale}. Timestamp ({timestamp})", IdSale, timestamp);
        
        var responseStockValidation= await _consumerStockValidationResponse.Consume(QueuesRabbitMQ.RESPONSE_VALIDATION_STOCK, IdSale, 30);
        if(responseStockValidation.IsFailed)
            return Result.Fail<CreateOrderResponse>(responseStockValidation.Errors);
        if (!responseStockValidation.Value.IsStockAvailable)
            return Result.Fail<CreateOrderResponse>("Stock is not available for one or more products in the order.");
        
        // Send message to decrease stock
        await _genericPublisher.Publish(request, QueuesRabbitMQ.REQUEST_DECREASE_STOCK, QueuesRabbitMQ.RESPONSE_DECREASE_STOCK, IdSale);
        _logger.LogInformation("POST api/Sales/Order - Stock validation successful, proceeding to decrease stock for Order ID: {IdSale}. Timestamp ({timestamp})", IdSale, timestamp);
        
        var resultDecreaseStock = await _consumerDecreaseStockResponse.Consume(QueuesRabbitMQ.RESPONSE_DECREASE_STOCK, IdSale, 30);
        if (resultDecreaseStock.IsFailed)
        {
            _logger.LogError("POST api/Sales/Order - Stock decreased request failed. Timestamp ({timestamp})", timestamp);
            return Result.Fail<CreateOrderResponse>(resultDecreaseStock.Errors);
        }
        if (!resultDecreaseStock.Value.IsSaleSuccess || resultDecreaseStock.ToResult().IsFailed)
        {
            _logger.LogError("POST api/Sales/Order - Stock decreased request failed. Timestamp ({timestamp})", timestamp);
            return Result.Fail<CreateOrderResponse>("An error occurred while decreasing stock.");
        }
        
        _logger.LogInformation("POST api/Sales/Order - Stock decreased successfully for Order ID: {IdSale}. Timestamp ({timestamp})", IdSale, timestamp);
        
        try
        {
            // Create Order
            var order = new Order(responseStockValidation.Value.IdOrder, request.Items, responseStockValidation.Value.ValueAmount);
            order.UpdateStatusOrder(StatusSale.COMPLETED);
            
            await _orderRepository.CreateOrder(order);
            await _orderRepository.SaveChangesAsync();
            _logger.LogInformation("POST api/Sales/Order - Order created successfully with ID: {IdOrder}. Timestamp ({timestamp})", order.IdSale, timestamp);
            
            var response = new CreateOrderResponse(order.IdSale, order.OrdemItens, order.TotalAmount, StatusSale.COMPLETED, order.CreatedAt);
            
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "POST api/Sales/Order - Error saving order to database. Timestamp ({timestamp})", timestamp);
            return Result.Fail<CreateOrderResponse>("Database error occurred while saving the order.");
        }
    }
}