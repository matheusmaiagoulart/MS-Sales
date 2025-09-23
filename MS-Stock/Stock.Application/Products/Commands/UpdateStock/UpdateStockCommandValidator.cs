using FluentValidation;

namespace Stock.Application.Products.Commands.UpdateStock;

public class UpdateStockCommandValidator : AbstractValidator<UpdateStockCommand>
{
    public UpdateStockCommandValidator()
    {
        RuleFor(x => x.IdProduct)
            .NotEmpty().WithMessage("IdProduct must be greater than or equal to zero.");
        
        RuleFor(x => x.Quantity)
            .NotEmpty().GreaterThan(0).WithMessage("Quantity must be greater than 1");
    }
    
}