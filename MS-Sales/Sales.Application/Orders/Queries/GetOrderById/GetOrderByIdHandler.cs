using FluentResults;
using FluentValidation;
using MediatR;
using Sales.Domain.Interfaces;
using Sales.Domain.Models;

namespace Sales.Application.Orders.Queries.GetOrderById;

public class GetOrderByIdHandler : IRequestHandler<GetOrderById, Result<Order>>
{
    IOrderRepository _orderRepository;
    IValidator<GetOrderById> _validator;
    public GetOrderByIdHandler(IOrderRepository orderRepository, IValidator<GetOrderById> validator)
    {
        _orderRepository = orderRepository;
        _validator = validator;
    }

    public async Task<Result<Order>> Handle(GetOrderById request, CancellationToken cancellationToken)
    {
        var resultValidation = _validator.Validate(request);
        if (!resultValidation.IsValid)
        {
            var errors = resultValidation.Errors
                .Select(e => new Error(e.ErrorMessage));
            return Result.Fail<Order>(errors);
        }
        try
        {
            var order = await _orderRepository.GetOrderById(request.IdOrder);
            if (order == null)
                return Result.Fail<Order>("Order not found");

            return Result.Ok(order);
        }
        catch (Exception ex)
        {
            return Result.Fail<Order>("Database error occurred while retrieving the order.");
        }
    }
}