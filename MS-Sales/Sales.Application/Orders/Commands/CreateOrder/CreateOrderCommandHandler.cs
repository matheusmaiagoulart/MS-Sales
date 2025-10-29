using System.Text.Json;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Sales.Application.Services;
using Sales.Application.Utils;
using Sales.Domain.DTOs;
using Sales.Domain.Interfaces;
using Sales.Domain.Models;

namespace Sales.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<CreateOrderResponse>>
{
    private readonly IValidator<CreateOrderCommand> _validator;
    private readonly ILogger<CreateOrderCommandHandler> _logger;
    private readonly IStockValidationRequest _stockValidationRequest;
    private readonly IStockDecreaseRequest _stockDecreaseRequest;
    private readonly IOrderRepository _orderRepository;

    public CreateOrderCommandHandler
    (
        ILogger<CreateOrderCommandHandler> logger,
        IValidator<CreateOrderCommand> validator,
        IOrderRepository orderRepository,
        IStockDecreaseRequest stockDecreaseRequest,
        IStockValidationRequest stockValidationRequest
    )
    {
        _logger = logger;
        _validator = validator;
        _orderRepository = orderRepository;
        _stockValidationRequest = stockValidationRequest;
        _stockDecreaseRequest = stockDecreaseRequest;
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
        Result<RequestCreateOrderValidationResponse> responseStockValidation = await _stockValidationRequest.SendStockValidationRequest(request, IdSale);
        if (responseStockValidation.IsFailed)
        {
            _logger.LogError("POST api/Sales/Order - Stock validation request failed. Timestamp ({timestamp})", timestamp);
            return Result.Fail<CreateOrderResponse>(responseStockValidation.Errors);
        }
        
        // Send message to decrease stock
        Result<UpdateStockResponse> updateStockResponse = await _stockDecreaseRequest.SendDecreaseStockRequest(request, IdSale);
        if (updateStockResponse.IsFailed)
        {
            _logger.LogError("POST api/Sales/Order - Stock validation request failed. Timestamp ({timestamp})", timestamp);
            return Result.Fail<CreateOrderResponse>(updateStockResponse.Errors);
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