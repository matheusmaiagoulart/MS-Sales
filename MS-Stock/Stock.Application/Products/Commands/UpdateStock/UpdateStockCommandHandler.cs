using System.Transactions;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Stock.Domain.Interfaces;

namespace Stock.Application.Products.Commands.UpdateStock;

public class UpdateStockCommandHandler : IRequestHandler<UpdateStockCommand, Result<UpdateStockCommandResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<UpdateStockCommandHandler> _logger;

    public UpdateStockCommandHandler(IProductRepository productRepository, ILogger<UpdateStockCommandHandler> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }


    public async Task<Result<UpdateStockCommandResponse>> Handle(UpdateStockCommand request, CancellationToken cancellationToken)
    {
        using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                var listItemsValidation = request.Items.DistinctBy(x => x.IdProduct).ToList();
                
                foreach (var item in listItemsValidation)
                {
                    var resultDecrease = await _productRepository.DecreaseStock(item.IdProduct, item.Quantity);
                    if (!resultDecrease)
                    {
                        // If any error occurs before transaction.Completed(), the transaction is rolled back
                        // transaction.Dispose(); occurs automatically
                        
                        _logger.LogError("Failed to decrease stock for IdProduct: " + item.IdProduct);
                        return Result.Fail<UpdateStockCommandResponse>("An error occurred and the Transaction was rolled back.");
                    }
                }
                
                transaction.Complete();
                _logger.LogInformation("Stock successfully decreased for all items.");
                return Result.Ok();

            }
            catch (Exception e)
            {
                _logger.LogError(e, "An exception occurred while updating stock.");
                Console.WriteLine(e);
                return Result.Fail(e.Message);
            }

        }
    }
}