using FluentValidation;

namespace Stock.Application.Product.Commands.UpdateStock;

public class UpdateStockCommandValidator : AbstractValidator<UpdateStockCommand>
{
    public UpdateStockCommandValidator()
    {
        RuleFor(x => x.Items.Count)
            .GreaterThan(0).NotEmpty().WithMessage("Items must contain at least one item and quantity grater than 0.");
    }
    
}