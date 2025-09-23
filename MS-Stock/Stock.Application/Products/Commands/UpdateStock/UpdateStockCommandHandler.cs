using FluentResults;
using MediatR;
using Stock.Domain.Interfaces;

namespace Stock.Application.Products.Commands.UpdateStock;

public class UpdateStockCommandHandler : IRequestHandler<UpdateStockCommand, Result>
{
    private readonly IProductRepository _productRepository;
    public UpdateStockCommandHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }
    
    
    public async Task<Result> Handle(UpdateStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetProductById(request.IdProduct);
        if (product == null)
           return Result.Fail("Product not found");
        
        product.DecreaseStock(request.Quantity);
        
        _productRepository.UpdateStock(product);
        await _productRepository.SaveChangesAsync();
        
        return Result.Ok();
    }
}