using Stock.Application.Products.Queries.StockValidation;
using Stock.Tests.Fixtures;

namespace Stock.Tests.Application.Products.Queries.StockValidation;

public class StockValidationValidationsTests : IClassFixture<ProductFixture>
{
    private ProductFixture _productFixture;

    public StockValidationValidationsTests(ProductFixture productFixture)
    {
        _productFixture = productFixture;
    }

    [Fact(DisplayName = "Must return success when Items list is valid")]
    public async Task StockValidationValidation_MustReturnSuccess_WhenItemsListIsValid()
    {
        // Arrange
        var query = _productFixture.StockValidationQuery();

        // Act
        var result = await _productFixture.StockValidationValidator.ValidateAsync(query);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "Must return error when Items list is empty")]
    public async Task StockValidationValidation_MustReturnError_WhenItemsListIsEmpty()
    {
        // Arrange
        var emptyItems = new List<OrderItemDTO>();
        var query = _productFixture.StockValidationQuery(items: emptyItems);

        // Act
        var result = await _productFixture.StockValidationValidator.ValidateAsync(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Items");
        Assert.Contains(result.Errors, x => x.ErrorMessage == "The items list cannot be empty.");
    }

    [Fact(DisplayName = "Must return error when Items have invalid quantity")]
    public async Task StockValidationValidation_MustReturnError_WhenItemsHaveInvalidQuantity()
    {
        // Arrange
        var invalidItems = new List<OrderItemDTO>
        {
            new OrderItemDTO { IdProduct = Guid.NewGuid(), Quantity = 0 },
            new OrderItemDTO { IdProduct = Guid.NewGuid(), Quantity = -1 }
        };
        var query = _productFixture.StockValidationQuery(items: invalidItems);

        // Act
        var result = await _productFixture.StockValidationValidator.ValidateAsync(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Items");
        Assert.Contains(result.Errors, x => x.ErrorMessage == "All items must have a quantity greater than zero.");
    }

    [Fact(DisplayName = "Must return success when all Items have valid quantity")]
    public async Task StockValidationValidation_MustReturnSuccess_WhenAllItemsHaveValidQuantity()
    {
        // Arrange
        var validItems = new List<OrderItemDTO>
        {
            new OrderItemDTO { IdProduct = Guid.NewGuid(), Quantity = 1 },
            new OrderItemDTO { IdProduct = Guid.NewGuid(), Quantity = 5 },
            new OrderItemDTO { IdProduct = Guid.NewGuid(), Quantity = 10 }
        };
        var query = _productFixture.StockValidationQuery(items: validItems);

        // Act
        var result = await _productFixture.StockValidationValidator.ValidateAsync(query);

        // Assert
        Assert.True(result.IsValid);
    }
}
