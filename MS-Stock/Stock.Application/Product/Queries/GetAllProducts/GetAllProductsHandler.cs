using FluentResults;
using MediatR;
using Stock.Domain.Models.Interfaces;

namespace Stock.Application.Products.Queries.GetAllProducts;

public class GetAllProductsHandler : IRequestHandler<GetAllProductsQuery, Result<IEnumerable<GetAllProductsResponse>>>
{
    private readonly IProductRepository _productRepository;

    public GetAllProductsHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<IEnumerable<GetAllProductsResponse>>> Handle(GetAllProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetAllProductsAsNoTracking();
        if (products == null)
            return Result.Fail<IEnumerable<GetAllProductsResponse>>("No products were found");

        var listResponse = products
            .Select(p => new GetAllProductsResponse(
                p.IdProduct,
                p.Name,
                p.Description,
                p.Price,
                p.StockQuantity,
                p.CreatedAt,
                p.UpdatedAt
            ))
            .ToList();

        return Result.Ok<IEnumerable<GetAllProductsResponse>>(listResponse);
    }
}