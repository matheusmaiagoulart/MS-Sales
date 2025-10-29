using FluentValidation;

namespace Stock.Application.Product.Queries.StockValidation;

public class StockValidationValidator : AbstractValidator<StockValidationQuery>
{
    public StockValidationValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("The items list cannot be empty.")
            .Must(items => items.All(item => item.Quantity > 0))
            .WithMessage("All items must have a quantity greater than zero.");
    }
}