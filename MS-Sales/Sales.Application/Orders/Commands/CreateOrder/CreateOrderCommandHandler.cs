using System.Text.Json;
using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Sales.Domain.DTOs;
using Sales.Domain.Models;
using Sales.Infrastructure.RabbitMQConfig.Consumer;
using Sales.Infrastructure.RabbitMQConfig.Publisher;

namespace Sales.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<CreateOrderResponse>>
{
    private readonly IValidator<CreateOrderCommand> _validator;
    private readonly RabbitMQPublisher _rabbitMQPublisher;
    private readonly ConfigRabbitMQConsumer _rabbitMQConsumer;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(IValidator<CreateOrderCommand> validator, RabbitMQPublisher rabbitMQPublisher,
        ConfigRabbitMQConsumer rabbitMQConsumer, ILogger<CreateOrderCommandHandler> logger)
    {
        _validator = validator;
        _rabbitMQPublisher = rabbitMQPublisher;
        _rabbitMQConsumer = rabbitMQConsumer;
        _logger = logger;
    }

    public async Task<Result<CreateOrderResponse>> Handle(CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        var timestamp = TimeZoneInfo
            .ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"))
            .ToString("yyyy-MM-dd'T'HH:mm:ss");

        _logger.LogInformation("POST api/Sales/Order - Starting order creation process. Timestamp ({timestamp})",
            timestamp);

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => new Error(e.ErrorMessage));

            return Result.Fail<CreateOrderResponse>(errors);
        }

        var IdSale = Guid.NewGuid();
        request.setIdOrder(IdSale);

        _rabbitMQPublisher.Publish(request, "orderValidationStockQueue", "orderResponseValidationStockQueue", IdSale);
        _logger.LogInformation(
            "POST api/Sales/Order - Validated order request, proceeding to stock validation for Order ID: {IdSale}. Timestamp ({timestamp})",
            IdSale, timestamp);
        Task.Delay(2000, cancellationToken).Wait(cancellationToken);

        try
        {
            // Esse trecho define tentativas maximas de tentar consumir a mensagem de resposta na fila, buscando pelo id da compra gerado no request
            
            var maxRetry = 4;
            var timeout = 5; // Tempo em segundos para esperar a resposta
           RequestCreateOrderValidationResponse responseStock = null;
            

            for (int i = 0; i < maxRetry; i++)
            {
                try
                {
                    // Tenta consumir a mensagem da fila, esperando no máximo "timeout" segundos
                    
                    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeout)); // Definindo o tempo limite de espera da resposta
                    responseStock = await Task.Run(() => 
                                         _rabbitMQConsumer.Consume("orderResponseValidationStockQueue", request.getIdOrder(), timeout));
                                    
                                    if(responseStock != null)
                                        break;
                                    
                }
                catch (OperationCanceledException e) // Exception lancada quando o tempo de espera acaba
                {
                    _logger.LogInformation(
                        "Attempt {Attempt} - No response received from stock validation service within {Timeout} seconds for Order ID: {IdSale}. Retrying... Timestamp ({timestamp})");
                    if(i == maxRetry -1)
                       break;
                }
                    await Task.Delay(2000, cancellationToken);
                    
            }
            if (responseStock == null)
            {
                _logger.LogError(
                    "POST api/Sales/Order - Timeout: No response from stock validation service after multiple attempts for Order ID: {IdSale}. Timestamp ({timestamp})",
                    IdSale, timestamp);
                return Result.Fail<CreateOrderResponse>("Timeout: No response from stock validation service.");
            }
            if (!responseStock.IsStockAvailable)
            {
                Console.WriteLine(JsonSerializer.Serialize(responseStock));
                _logger.LogWarning("POST api/Sales/Order - Stock is not available for one or more items. " + "\n" +
                                   "The order ID: {IdSale}. Timestamp ({timestamp})", IdSale, timestamp);
                return Result.Fail<CreateOrderResponse>("Stock is not available for one or more items.");
            }
            

            var order = new Order(responseStock.IdOrder, request.Items, responseStock.ValueAmount);
            var response = new CreateOrderResponse(order.IdSale, order.OrdemItens, order.TotalAmount, order.Status,
                order.CreatedAt);
            _logger.LogInformation(
                "POST api/Sales/Order - Order created successfully with ID: {IdOrder}. Timestamp ({timestamp})",
                response.IdOrder, timestamp);
            return Result.Ok(response);
        }
        catch (OperationCanceledException e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        
        
    }
}