using System.Transactions;
using FluentResults;
using MediatR;
using Stock.Domain.Interfaces;

namespace Stock.Application.Products.Commands.UpdateStock;

public class UpdateStockCommandHandler : IRequestHandler<UpdateStockCommand, Result<UpdateStockCommandResponse>>
{
    private readonly IProductRepository _productRepository;

    public UpdateStockCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
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
                        Console.WriteLine("Failed to decrease stock for IdProduct: " + item.IdProduct);
                        return Result.Fail<UpdateStockCommandResponse>("Failed to decrease stock for IdProduct: " + item.IdProduct);
                        
                    }
                    // resposta do pagamento
                    
                }
                transaction.Complete();
                Console.WriteLine("Stock successfully decreased for all items.");
                return Result.Ok();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Result.Fail(e.Message);
            }

        }
    }
}