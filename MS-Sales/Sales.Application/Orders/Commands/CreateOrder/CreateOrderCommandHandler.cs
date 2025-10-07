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
    private readonly IRabbitMqPublisher _rabbitMQPublisher;
    private readonly IStockValidationService _stockValidationService;
    private readonly ILogger<CreateOrderCommandHandler> _logger;
    private readonly IDecreaseStockResponseConsumer _consumerDecreaseStockResponse;
    private readonly IOrderRepository _orderRepository;

    public CreateOrderCommandHandler
    (
        IValidator<CreateOrderCommand> validator, //FluentValidation
        IRabbitMqPublisher _rabbitMqPublisher,
        IStockValidationService stockValidationService, // Serviço para validar o estoque
        ILogger<CreateOrderCommandHandler> logger,
        IDecreaseStockResponseConsumer consumerDecreaseStockResponse, // Serviço para consumir a resposta de diminuição de estoque
        IOrderRepository orderRepository
    )
    {
        _validator = validator;
        _rabbitMQPublisher = _rabbitMqPublisher;
        _stockValidationService = stockValidationService;
        _logger = logger;
        _consumerDecreaseStockResponse = consumerDecreaseStockResponse;
        _orderRepository = orderRepository;
    }

    public async Task<Result<CreateOrderResponse>> Handle(CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        var timestamp = CommonData.GetTimestamp();

        _logger.LogInformation("POST api/Sales/Order - Starting order creation process. Timestamp ({timestamp})",
            timestamp);

        // FluentResult Validation
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => new Error(e.ErrorMessage));

            return Result.Fail<CreateOrderResponse>(errors);
        }

        var IdSale = Guid.NewGuid();
        request.setIdOrder(IdSale);

        await _rabbitMQPublisher.Publish(request, QueuesRabbitMQ.REQUEST_VALIDATION_STOCK,
            QueuesRabbitMQ.RESPONSE_VALIDATION_STOCK, IdSale);
        _logger.LogInformation(
            "POST api/Sales/Order - Validated order request, proceeding to stock validation for Order ID: {IdSale}. Timestamp ({timestamp})",
            IdSale, timestamp);

        var responseStockValidation =
            await _stockValidationService.ValidateStockAsync<RequestCreateOrderValidationResponse>(request, IdSale,
                QueuesRabbitMQ.RESPONSE_VALIDATION_STOCK, cancellationToken);
        if (!responseStockValidation.Value.IsStockAvailable)
        {
            return Result.Fail<CreateOrderResponse>("Stock is not available for one or more products in the order.");
        }
        
        await _rabbitMQPublisher.Publish(request, QueuesRabbitMQ.REQUEST_DECREASE_STOCK, QueuesRabbitMQ.RESPONSE_DECREASE_STOCK, IdSale);
        Console.WriteLine("Pedido enviado para decrease stock" + JsonSerializer.Serialize(request));
        
        var resultDecreaseStock = await _consumerDecreaseStockResponse.Consume(QueuesRabbitMQ.RESPONSE_DECREASE_STOCK, IdSale, 5);
        if (!resultDecreaseStock.IsSaleSuccess || resultDecreaseStock.ToResult().IsFailed)
        {
            return Result.Fail("An error occurred while decreasing stock.");
        }
        Console.WriteLine(resultDecreaseStock + " Resultado do decrease stock");
        _logger.LogInformation(
            "POST api/Sales/Order - Stock decreased successfully for Order ID: {IdSale}. Timestamp ({timestamp})",
            IdSale, timestamp);
        


        var order = new Order(responseStockValidation.Value.IdOrder, request.Items,
            responseStockValidation.Value.ValueAmount);
        var response = new CreateOrderResponse(order.IdSale, order.OrdemItens, order.TotalAmount, StatusSale.COMPLETED,
            order.CreatedAt);
        order.UpdateStatusOrder(StatusSale.COMPLETED);
        await _orderRepository.CreateOrder(order);
        await _orderRepository.SaveChangesAsync();
        _logger.LogInformation(
            "POST api/Sales/Order - Order created successfully with ID: {IdOrder}. Timestamp ({timestamp})",
            response.IdOrder, timestamp);
        return Result.Ok(response);
    }
}