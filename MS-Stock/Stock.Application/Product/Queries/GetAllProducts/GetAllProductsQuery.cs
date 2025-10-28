using FluentResults;
using MediatR;

namespace Stock.Application.Products.Queries.GetAllProducts;

public record GetAllProductsQuery() 
    : IRequest<Result<IEnumerable<GetAllProductsResponse?>>>;