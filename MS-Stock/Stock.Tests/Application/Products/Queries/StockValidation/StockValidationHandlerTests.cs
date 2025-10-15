using Moq;
using Stock.Application.Products.Queries.StockValidation;
using Stock.Tests.Fixtures;

namespace Stock.Tests.Application.Products.Queries.StockValidation;

public class StockValidationHandlerTests : IClassFixture<ProductFixture>
{
    private ProductFixture _productFixture;

    public StockValidationHandlerTests(ProductFixture productFixture)
    {
        _productFixture = productFixture;
    }

    [Fact(DisplayName = "Must return SUCCESSFUL validation when all products have sufficient stock")]
    public async Task StockValidation_MustReturnSuccessful_WhenAllProductsHaveSufficientStock()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();
        var items = new List<OrderItemDTO>
        {
            new OrderItemDTO { IdProduct = productId1, Quantity = 2 },
            new OrderItemDTO { IdProduct = productId2, Quantity = 3 }
        };
        var query = _productFixture.StockValidationQuery(orderId, items);

        _productFixture.MockRepository.Setup(x => x.GetProductPriceIfStockAvailable(productId1, 2))
            .ReturnsAsync(10.50m);
        _productFixture.MockRepository.Setup(x => x.GetProductPriceIfStockAvailable(productId2, 3))
            .ReturnsAsync(15.00m);

        // Act
        var service = _productFixture.StockValidationHandler();
        var result = await service.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(orderId, result.Value.IdOrder);
        Assert.True(result.Value.Available);
        Assert.Equal(66.00m, result.Value.TotalAmount);
        _productFixture.MockRepository.Verify(x => x.GetProductPriceIfStockAvailable(productId1, 2), Times.Once);
        _productFixture.MockRepository.Verify(x => x.GetProductPriceIfStockAvailable(productId2, 3), Times.Once);
    }

    [Fact(DisplayName = "Must return FAILURE when product has insufficient stock")]
    public async Task StockValidation_MustReturnFailure_WhenProductHasInsufficientStock()
    {
        // Arrange
        _productFixture.MockRepository.Reset();
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var items = new List<OrderItemDTO>
        {
            new OrderItemDTO { IdProduct = productId, Quantity = 5 }
        };
        var query = _productFixture.StockValidationQuery(orderId, items);

        _productFixture.MockRepository.Setup(x => x.GetProductPriceIfStockAvailable(productId, 5))
            .ReturnsAsync(0m);

        // Act
        var service = _productFixture.StockValidationHandler();
        var result = await service.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, x => x.Message.Contains($"Stock is not available for product ID: {productId}"));
        _productFixture.MockRepository.Verify(x => x.GetProductPriceIfStockAvailable(productId, 5), Times.Once);
    }

    [Fact(DisplayName = "Must handle duplicate products by using distinct items")]
    public async Task StockValidation_MustHandleDuplicateProducts_ByUsingDistinctItems()
    {
        // Arrange
        _productFixture.MockRepository.Reset();
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var items = new List<OrderItemDTO>
        {
            new OrderItemDTO { IdProduct = productId, Quantity = 2 },
            new OrderItemDTO { IdProduct = productId, Quantity = 3 }
        };
        var query = _productFixture.StockValidationQuery(orderId, items);

        _productFixture.MockRepository.Setup(x => x.GetProductPriceIfStockAvailable(productId, 2))
            .ReturnsAsync(10.00m);

        // Act
        var service = _productFixture.StockValidationHandler();
        var result = await service.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.True(result.Value.Available);
        Assert.Equal(20.00m, result.Value.TotalAmount); // Only first item should be processed due to DistinctBy
        _productFixture.MockRepository.Verify(x => x.GetProductPriceIfStockAvailable(productId, 2), Times.Once);
        _productFixture.MockRepository.Verify(x => x.GetProductPriceIfStockAvailable(productId, 3), Times.Never);
    }
}
