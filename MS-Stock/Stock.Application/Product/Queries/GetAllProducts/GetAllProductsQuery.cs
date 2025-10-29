using FluentResults;
using MediatR;

namespace Stock.Application.Product.Queries.GetAllProducts;

public record GetAllProductsQuery() 
    : IRequest<Result<IEnumerable<GetAllProductsResponse?>>>;