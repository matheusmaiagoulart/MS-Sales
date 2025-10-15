using Stock.Tests.Fixtures;

namespace Stock.Tests.Application.Products.Queries.GetProductById;

public class GetProductByIdValidationsTests : IClassFixture<ProductFixture>
{
    private ProductFixture _productFixture;

    public GetProductByIdValidationsTests(ProductFixture productFixture)
    {
        _productFixture = productFixture;
    }

    [Fact(DisplayName = "Must return success when Id is valid")]
    public async Task GetProductByIdValidation_MustReturnSuccess_WhenIdIsValid()
    {
        // Arrange
        var query = _productFixture.GetProductByIdQuery();

        // Act
        var result = await _productFixture.GetProductByIdValidator.ValidateAsync(query);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "Must return error when Id is empty")]
    public async Task GetProductByIdValidation_MustReturnError_WhenIdIsEmpty()
    {
        // Arrange
        var query = _productFixture.GetProductByIdQuery(Guid.Empty);

        // Act
        var result = await _productFixture.GetProductByIdValidator.ValidateAsync(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Id");
        Assert.Contains(result.Errors, x => x.ErrorMessage == "Id must not be empty.");
    }
}
