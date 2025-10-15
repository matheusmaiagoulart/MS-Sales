using Moq;
using Stock.Domain.Models;
using Stock.Tests.Fixtures;

namespace Stock.Tests.Application.Products.Commands.CreateProduct;

public class CreateProductHandlerTests : IClassFixture<ProductFixture>
{
    
    private ProductFixture _productFixture;

    public CreateProductHandlerTests(ProductFixture productFixture)
    {
        _productFixture = productFixture;
    }

    [Fact(DisplayName = "Must create a SUCCESSFUL Product when have no errors")]
    public async Task CreateProduct_MustReturnSucessful_WhenHaveNoErros()
    {
        // Arrange
        var CreateProductDTO = _productFixture.CreateProductCommandDTO();
        var CreateProductModel = _productFixture.CreateProductModel(CreateProductDTO.Name, CreateProductDTO.Description, CreateProductDTO.Price, CreateProductDTO.StockQuantity);

        _productFixture.MockRepository.Setup(x => x.CreateProduct(CreateProductModel)).Returns(Task.CompletedTask);

        // Act 
        var service = _productFixture.CreateProductCommandHandler();
        var result = service.Handle(CreateProductDTO, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsCompleted);
        Assert.Equal(CreateProductDTO.Name, result.Result.Value.Name);
        Assert.Equal(CreateProductDTO.Description, result.Result.Value.Description);
        Assert.Equal(CreateProductDTO.Price, result.Result.Value.Price);
        Assert.Equal(CreateProductDTO.StockQuantity, result.Result.Value.StockQuantity);
        _productFixture.MockRepository.Verify(x => x.CreateProduct(It.IsAny<Product>()), Times.Once);
        _productFixture.MockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
    
    [Fact(DisplayName = "Must return validation errors when command is invalid")]
    public async Task CreateProduct_MustReturnValidationErrors_WhenCommandIsInvalid()
    {
        _productFixture.MockRepository.Reset();
        // Arrange
        var invalidCommand = _productFixture.CreateProductCommandDTO(name: "", description:""); 
        // Act
        var service = _productFixture.CreateProductCommandHandler();
        var result = await service.Handle(invalidCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.NotEmpty(result.Errors);
        _productFixture.MockRepository.Verify(x => x.CreateProduct(It.IsAny<Product>()), Times.Never);
        _productFixture.MockRepository.Verify(x => x.SaveChangesAsync(), Times.Never);
    }
    
    [Fact(DisplayName = "Must return Network error when database is not reachable")]
    public async Task CreateProduct_MustReturnError_WhenDatabaseIsNotReachable()
    {
        _productFixture.MockRepository.Reset();
        // Arrange
        var command = _productFixture.CreateProductCommandDTO();
        
        _productFixture.MockRepository.Setup(x => x.SaveChangesAsync())
            .Throws(new Exception());
        
        // Act
        var service = _productFixture.CreateProductCommandHandler();
        var result = await service.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
        _productFixture.MockRepository.Verify(x => x.CreateProduct(It.IsAny<Product>()), Times.Once);
        _productFixture.MockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
    
    
}