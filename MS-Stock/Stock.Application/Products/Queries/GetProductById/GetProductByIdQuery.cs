using FluentResults;
using MediatR;

namespace Stock.Application.Products.Queries.GetProductById;

public record GetProductByIdQuery(Guid Id) 
    : IRequest<Result<GetProductByIdResponse>>;