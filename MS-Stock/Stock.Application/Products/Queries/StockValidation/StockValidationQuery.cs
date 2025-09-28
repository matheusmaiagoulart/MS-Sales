using FluentResults;
using MediatR;

namespace Stock.Application.Products.Queries.StockValidation;

public record StockValidationQuery(Guid IdOrder, List<OrdemItemDTO> Items)
    : IRequest<Result<StockValidationResponse>>;