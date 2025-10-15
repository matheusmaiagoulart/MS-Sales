using FluentValidation;

namespace Sales.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotNull().WithMessage("Items cannot be null or empty.")
            .NotEmpty().WithMessage("Items cannot be null or empty.");
    }
}