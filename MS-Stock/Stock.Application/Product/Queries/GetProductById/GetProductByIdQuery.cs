using FluentResults;
using MediatR;

namespace Stock.Application.Product.Queries.GetProductById;

public class GetProductByIdQuery() : IRequest<Result<GetProductByIdResponse>>
{
    public Guid Id { get; set; }
    
    public GetProductByIdQuery(Guid id) : this()
    {
        Id = id;
    }
}