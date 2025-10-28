using FluentResults;
using MediatR;
using Stock.Domain.Models.Interfaces;

namespace Stock.Application.Products.Queries.StockValidation;

public class StockValidationHandler : IRequestHandler<StockValidationQuery, Result<StockValidationResponse>>
{
    private readonly IProductRepository _productRepository;
    public StockValidationHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }
    public async Task<Result<StockValidationResponse>> Handle(StockValidationQuery request, CancellationToken cancellationToken)
    {
        
        var listItemsValidation = request.Items.DistinctBy(x => x.IdProduct).ToList();
        decimal totalAmout = 0;
        
        var response = new StockValidationResponse (request.IdOrder, false, 0);
        
        for (int i = 0; i < listItemsValidation.Count(); i++)
        {
            var result = await _productRepository
                .GetProductPriceIfStockAvailable(listItemsValidation.ElementAt(i).IdProduct,
                    listItemsValidation.ElementAt(i).Quantity);

            if (result == 0)
                return Result.Fail<StockValidationResponse>(
                    new Error("Stock is not available for product ID: " +
                              listItemsValidation.ElementAt(i).IdProduct));
            
                totalAmout += result * listItemsValidation.ElementAt(i).Quantity;
        }
            
        
        response.Available = true;
        response.TotalAmount = totalAmout;
        return Result.Ok(response);
    }
}