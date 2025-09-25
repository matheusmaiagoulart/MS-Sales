using FluentResults;
using MediatR;
using Stock.Domain.Interfaces;

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
        Console.WriteLine("Iniciando validação de estoque...");
        var listItemsValidation = request.Items.ToList().Distinct();
        decimal totalAmout = 0;
        var response = new StockValidationResponse {Available = false, TotalAmount = 0};
        for (int i = 0; i < listItemsValidation.Count(); i++)
        {
            var result = await _productRepository
                .GetProductPriceIfStockAvailable(listItemsValidation.ElementAt(i).IdProduct,
                    listItemsValidation.ElementAt(i).Quantity);

            if (result == null)
                return Result.Fail(response + "");
            
            totalAmout += result.Value * listItemsValidation.ElementAt(i).Quantity;
            
        }
        Console.WriteLine("Estoque disponível para todos os itens." + totalAmout);
        response.Available = true;
        response.TotalAmount = totalAmout;
        return Result.Ok(response);
    }
}