using FluentResults;
using FluentValidation;
using MediatR;
using Sales.Domain.Interfaces;
using Sales.Domain.Models;

namespace Sales.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<CreateOrderResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IValidator<CreateOrderCommand> _validator;
    public CreateOrderCommandHandler(IOrderRepository orderRepository, IValidator<CreateOrderCommand> validator)
    {
        orderRepository = _orderRepository;
        _validator = validator;
    }
    public async Task<Result<CreateOrderResponse>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => new Error(e.ErrorMessage));
            return Result.Fail<CreateOrderResponse>(errors);
        }

        decimal valueAmount = request.Items.Count * 3;
        var order = new Order(request.Items, valueAmount);

        var response = new CreateOrderResponse(order.IdSale, order.OrdemItens, order.TotalAmount, order.Status, order.CreatedAt);
        return Result.Ok(response);
    }
}