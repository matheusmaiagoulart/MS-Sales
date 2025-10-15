using Stock.Tests.Fixtures;

namespace Stock.Tests.Application.Products.Commands.UpdateStock;

public class UpdateStockValidationsTests : IClassFixture<ProductFixture>
{
    private ProductFixture _productFixture;
    public UpdateStockValidationsTests(ProductFixture productFixture)
    {
        _productFixture = productFixture;
    }
    
    
    [Fact(DisplayName = "Must return success when Data is valid")]
    public async Task UpdateStockValidation_MustReturnSucess_WhenDataIsValid()
    {
        // Arrange
        var command = _productFixture.UpdateStockCommandDTO();

        // Act
        var result = await _productFixture.UpdateStockCommandValidator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
    }
    
    [Fact(DisplayName = "Must return error when have no one items in the list")]
    public async Task UpdateStockValidation_MustReturnError_WhenHaveNoItemsInTheLists()
    {
        // Arrange
        var command = _productFixture.UpdateStockCommandDTO(Guid.NewGuid(), countItems:0);

        // Act
        var result = await _productFixture.UpdateStockCommandValidator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Items.Count");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Items must contain at least one item and quantity grater than 0.");
    }
    
}