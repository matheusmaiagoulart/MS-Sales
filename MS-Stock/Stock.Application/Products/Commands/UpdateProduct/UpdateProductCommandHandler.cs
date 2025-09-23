using FluentResults;
using FluentValidation;
using MediatR;
using Stock.Domain.Interfaces;

namespace Stock.Application.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<UpdateProductResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly IValidator<UpdateProductCommand> _validatorUpdateProduct;

    public UpdateProductCommandHandler(IProductRepository productRepository,
        IValidator<UpdateProductCommand> validatorUpdateProduct)
    {
        _productRepository = productRepository;
        _validatorUpdateProduct = validatorUpdateProduct;
    }


    public async Task<Result<UpdateProductResponse>> Handle(UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validatorUpdateProduct.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(e => new Error(e.ErrorMessage));
            return Result.Fail<UpdateProductResponse>(errors);
        }

        var product = await _productRepository.GetProductById(request.Id);
        if (product == null)
            return Result.Fail("Product not found");

        product.UpdateProduct
        (
            request.Name ?? product.Name,
            request.Description ?? product.Description,
            request.Price ?? product.Price
        );

        _productRepository.UpdateStock(product);
        await _productRepository.SaveChangesAsync();

        var response = new UpdateProductResponse(
            product.IdProduct,
            product.Name,
            product.Description,
            product.Price,
            product.StockQuantity,
            product.CreatedAt,
            product.UpdatedAt
        );

        return Result.Ok(response);
    }
}