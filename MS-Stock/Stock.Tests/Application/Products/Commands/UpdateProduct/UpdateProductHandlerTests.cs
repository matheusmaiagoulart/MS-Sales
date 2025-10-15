using FluentResults;
using Moq;
using Stock.Domain.Models;
using Stock.Tests.Fixtures;

namespace Stock.Tests.Application.Products.Commands.UpdateProduct;

public class UpdateProductHandlerTests : IClassFixture<ProductFixture>
{
    private ProductFixture _productFixture;

    public UpdateProductHandlerTests(ProductFixture productFixture)
    {
        _productFixture = productFixture;
    }
    
    [Fact(DisplayName = "Must do a update Product SUCCESSFUL when have no errors")]
    public async Task UpdateProduct_MustReturnSucessful_WhenHaveNoErros()
    {_productFixture.MockRepository.Reset();
        // Arrange
        var ProductModelBefore = _productFixture.CreateProductModel();

        _productFixture.MockRepository.Setup(x => x.GetProductById(ProductModelBefore.IdProduct))
            .ReturnsAsync(ProductModelBefore);

        var UpdatedProductModel = _productFixture.UpdateProductCommandDTO(idProduct: ProductModelBefore.IdProduct,
            name: "TesteUpdate", description: "TesteUpdateDescription", ProductModelBefore.Price, ProductModelBefore.StockQuantity);
        var ProductModelAfter = _productFixture.CreateProductModel(UpdatedProductModel.Name, UpdatedProductModel.Description, UpdatedProductModel.Price, UpdatedProductModel.StockQuantity);

        _productFixture.MockRepository.Setup(x => x.UpdateProduct(ProductModelAfter));
        _productFixture.MockRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);
        
        // Act 
        var service = _productFixture.UpdateProductCommandHandler();
        var result = service.Handle(UpdatedProductModel, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsCompleted);
        Assert.Equal(UpdatedProductModel.Name, result.Result.Value.Name);
        Assert.Equal(UpdatedProductModel.Description, result.Result.Value.Description);
        Assert.Equal(UpdatedProductModel.Price, result.Result.Value.Price);
        Assert.Equal(UpdatedProductModel.StockQuantity, result.Result.Value.StockQuantity);
        
        _productFixture.MockRepository.Verify(x => x.UpdateProduct(It.IsAny<Product>()), Times.Once);
        _productFixture.MockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
    
    [Fact(DisplayName = "Must return error when have a exception")]
    public async Task UpdateProduct_MustReturnError_WhenHaveAException()
    {
        _productFixture.MockRepository.Reset();
        
        // Arrange
        var ProductModelBefore = _productFixture.CreateProductModel();

        _productFixture.MockRepository.Setup(x => x.GetProductById(ProductModelBefore.IdProduct))
            .ReturnsAsync(ProductModelBefore);

        var UpdatedProductModel = _productFixture.UpdateProductCommandDTO(idProduct: ProductModelBefore.IdProduct,
            name: "TesteUpdate", description: "TesteUpdateDescription", ProductModelBefore.Price, ProductModelBefore.StockQuantity);
        var ProductModelAfter = _productFixture.CreateProductModel(UpdatedProductModel.Name, UpdatedProductModel.Description, UpdatedProductModel.Price, UpdatedProductModel.StockQuantity);

        _productFixture.MockRepository.Setup(x => x.UpdateProduct(ProductModelAfter));
        _productFixture.MockRepository.Setup(x => x.SaveChangesAsync()).Throws(new Exception());
        
        // Act 
        var service = _productFixture.UpdateProductCommandHandler();
        var result = await service.Handle(UpdatedProductModel, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsFailed);
        Assert.NotEmpty(result.Errors);
        _productFixture.MockRepository.Verify(x => x.UpdateProduct(It.IsAny<Product>()), Times.Once);
        _productFixture.MockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);

    }
    
}