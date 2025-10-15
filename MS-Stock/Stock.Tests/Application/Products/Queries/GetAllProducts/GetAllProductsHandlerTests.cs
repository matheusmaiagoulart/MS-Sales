using Moq;
using Stock.Domain.Models;
using Stock.Tests.Fixtures;

namespace Stock.Tests.Application.Products.Queries.GetAllProducts;

public class GetAllProductsHandlerTests : IClassFixture<ProductFixture>
{
    private ProductFixture _productFixture;

    public GetAllProductsHandlerTests(ProductFixture productFixture)
    {
        _productFixture = productFixture;
    }

    [Fact(DisplayName = "Must return SUCCESSFUL list of products when products exist")]
    public async Task GetAllProducts_MustReturnSuccessful_WhenProductsExist()
    {
        // Arrange
        _productFixture.MockRepository.Reset();
        var query = _productFixture.GetAllProductsQuery();
        var products = new List<Product>
        {
            _productFixture.CreateProductModel(),
            _productFixture.CreateProductModel()
        };

        _productFixture.MockRepository.Setup(x => x.GetAllProductsAsNoTracking()).ReturnsAsync(products);

        // Act
        var service = _productFixture.GetAllProductsHandler();
        var result = await service.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count());
        _productFixture.MockRepository.Verify(x => x.GetAllProductsAsNoTracking(), Times.Once);
    }

    [Fact(DisplayName = "Must return FAILURE when no products are found")]
    public async Task GetAllProducts_MustReturnFailure_WhenNoProductsFound()
    {
        // Arrange
        _productFixture.MockRepository.Reset();
        var query = _productFixture.GetAllProductsQuery();

        _productFixture.MockRepository.Setup(x => x.GetAllProductsAsNoTracking()).ReturnsAsync((List<Product>)null);

        // Act
        var service = _productFixture.GetAllProductsHandler();
        var result = await service.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, x => x.Message == "No products were found");
        _productFixture.MockRepository.Verify(x => x.GetAllProductsAsNoTracking(), Times.Once);
    }

    [Fact(DisplayName = "Must return SUCCESSFUL empty list when products list is empty")]
    public async Task GetAllProducts_MustReturnSuccessfulEmptyList_WhenProductsListIsEmpty()
    {
        // Arrange
        _productFixture.MockRepository.Reset();
        var query = _productFixture.GetAllProductsQuery();
        var emptyProducts = new List<Product>();

        _productFixture.MockRepository.Setup(x => x.GetAllProductsAsNoTracking()).ReturnsAsync(emptyProducts);

        // Act
        var service = _productFixture.GetAllProductsHandler();
        var result = await service.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
        _productFixture.MockRepository.Verify(x => x.GetAllProductsAsNoTracking(), Times.Once);
    }
}
