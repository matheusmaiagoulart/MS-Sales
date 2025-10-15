using Moq;
using Stock.Domain.Models;
using Stock.Tests.Fixtures;

namespace Stock.Tests.Application.Products.Queries.GetProductById;

public class GetProductByIdHandlerTests : IClassFixture<ProductFixture>
{
    private ProductFixture _productFixture;

    public GetProductByIdHandlerTests(ProductFixture productFixture)
    {
        _productFixture = productFixture;
    }

    [Fact(DisplayName = "Must return SUCCESSFUL product when product exists")]
    public async Task GetProductById_MustReturnSuccessful_WhenProductExists()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var query = _productFixture.GetProductByIdQuery(productId);
        var product = _productFixture.CreateProductModel();
        product.GetType().GetProperty("IdProduct")?.SetValue(product, productId);

        _productFixture.MockRepository.Setup(x => x.GetProductByIdAsNoTracking(productId)).ReturnsAsync(product);

        // Act
        var service = _productFixture.GetProductByIdHandler();
        var result = await service.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(productId, result.Value.IdProduct);
        Assert.Equal(product.Name, result.Value.Name);
        Assert.Equal(product.Description, result.Value.Description);
        Assert.Equal(product.Price, result.Value.Price);
        Assert.Equal(product.StockQuantity, result.Value.StockQuantity);
        _productFixture.MockRepository.Verify(x => x.GetProductByIdAsNoTracking(productId), Times.Once);
    }

    [Fact(DisplayName = "Must return FAILURE when product not found")]
    public async Task GetProductById_MustReturnFailure_WhenProductNotFound()
    {
        // Arrange
        _productFixture.MockRepository.Reset();
        var productId = Guid.NewGuid();
        var query = _productFixture.GetProductByIdQuery(productId);

        _productFixture.MockRepository.Setup(x => x.GetProductByIdAsNoTracking(productId)).ReturnsAsync((Product)null);

        // Act
        var service = _productFixture.GetProductByIdHandler();
        var result = await service.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, x => x.Message == "Product not found");
        _productFixture.MockRepository.Verify(x => x.GetProductByIdAsNoTracking(productId), Times.Once);
    }
}
