using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;
using Stock.Application.Interfaces;
using Stock.Application.Product.Queries.StockValidation;

namespace Stock.Application.Product.Commands.UpdateStock;

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
        var itensReserved = new List<OrderItemDto>();
        try
        {
            var listItemsValidation = request.Items.DistinctBy(x => x.IdProduct).ToList();

            foreach (var item in listItemsValidation)
            {
                var resultDecrease =
                    await _productRepository.TryReserve(request.IdOrder, item.IdProduct, item.Quantity);
                if (!resultDecrease)
                {
                    _logger.LogError("Failed to decrease stock for IdProduct: " + item.IdProduct);
                    throw new Exception("Failed to decrease stock");
                }

                itensReserved.Add(item);
            }

            foreach (var item in itensReserved)
            {
                await _productRepository.ConfirmReservation(request.IdOrder, item.IdProduct, item.Quantity);
            }

            _logger.LogInformation("Stock successfully decreased for all items.");
            return Result.Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An exception occurred while updating stock.");
            foreach (var item in itensReserved)
            {
                await _productRepository.CancelReservation(request.IdOrder, item.IdProduct, item.Quantity);
            }

            return Result.Fail<UpdateStockCommandResponse>(
                "An error occurred and the Transaction was rolled back.");
        }
    }
}