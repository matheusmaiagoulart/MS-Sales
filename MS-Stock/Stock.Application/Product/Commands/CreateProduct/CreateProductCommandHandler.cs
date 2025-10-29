using FluentResults;
using FluentValidation;
using MediatR;
using Stock.Application.Interfaces;
using Result = FluentResults.Result;

namespace Stock.Application.Product.Commands.CreateProduct;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<CreateProductResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly IValidator<CreateProductCommand> _validatorProduct;
    public CreateProductCommandHandler(IProductRepository productRepository,
        IValidator<CreateProductCommand> validatorProduct)
    {
        _productRepository = productRepository;
        _validatorProduct = validatorProduct;
    }

    public async Task<Result<CreateProductResponse>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var validationResult = await _validatorProduct.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => new Error(e.ErrorMessage));
            return Result.Fail<CreateProductResponse>(errors);
        }

        try
        {
            var product = new Domain.Models.Product
            (
                request.Name,
                request.Description,
                request.Price,
                request.StockQuantity
            );

            await _productRepository.CreateProduct(product);
            await _productRepository.SaveChangesAsync();

            var response = new CreateProductResponse(
                product.IdProduct,
                product.Name,
                product.Description,
                product.Price,
                product.StockQuantity,
                product.CreatedAt);

            return Result.Ok(response);
        }
        catch (Exception e)
        {
            return Result.Fail<CreateProductResponse>(e.Message);
            
        }
        
    }
}