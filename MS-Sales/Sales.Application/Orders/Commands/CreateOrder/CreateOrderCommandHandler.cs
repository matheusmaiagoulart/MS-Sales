using FluentResults;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Sales.Domain.Interfaces;
using Sales.Domain.Models;
using Sales.Infrastructure.RabbitMQConfig;

namespace Sales.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<CreateOrderResponse>>
{
    private readonly IValidator<CreateOrderCommand> _validator;
    private readonly RabbitMQPublisher _rabbitMQPublisher;
    public CreateOrderCommandHandler(IValidator<CreateOrderCommand> validator, RabbitMQPublisher rabbitMQPublisher)
    {
        
        _validator = validator;
        _rabbitMQPublisher = rabbitMQPublisher;
    }
    public async Task<Result<CreateOrderResponse>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => new Error(e.ErrorMessage));
            
            return Result.Fail<CreateOrderResponse>(errors);
        }
        
        decimal valueAmount = 0;
        var order = new Order(request.Items, valueAmount);
        await _rabbitMQPublisher.Publish(request, "orderValidationStockQueue");

        var response = new CreateOrderResponse(order.IdSale, order.OrdemItens, order.TotalAmount, order.Status, order.CreatedAt);
        return Result.Ok(response);
    }
}