using FluentResults;
using FluentValidation;
using MediatR;
using Stock.Application.Interfaces;

namespace Stock.Application.Product.Queries.GetProductById;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, Result<GetProductByIdResponse>>
{
    private readonly IValidator<GetProductByIdQuery> _validator;
    private readonly IProductRepository _productRepository;
    public GetProductByIdHandler(IValidator<GetProductByIdQuery> validator, IProductRepository productRepository)
    {
        _validator = validator;
        _productRepository = productRepository;
    }
    
    public async Task<Result<GetProductByIdResponse>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetProductByIdAsNoTracking(request.Id);
        if (product == null)
            return Result.Fail<GetProductByIdResponse>("Product not found");

        var response = new GetProductByIdResponse
        (
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