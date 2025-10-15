using FluentValidation;

namespace Sales.Application.Orders.Queries.GetOrderById;

public class GetOrderByIdValidator : AbstractValidator<GetOrderById>
{
    public GetOrderByIdValidator()
    {
        RuleFor(x => x.IdOrder)
            .NotEmpty().WithMessage("IdOrder is required.")
            .NotEqual(Guid.Empty).WithMessage("IdOrder must be a valid GUID.");
    }
}