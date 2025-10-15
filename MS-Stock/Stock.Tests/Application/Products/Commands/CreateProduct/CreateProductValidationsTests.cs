using Stock.Tests.Fixtures;

namespace Stock.Tests.Application.Products.Commands.CreateProduct;

public class CreateProductValidationsTests : IClassFixture<ProductFixture>
{
    private ProductFixture _productFixture;

    public CreateProductValidationsTests(ProductFixture productFixture)
    {
        _productFixture = productFixture;
    }

    [Fact(DisplayName = "Must return success when Data is valid")]
    public async Task CreateProductValidation_MustReturnSucess_WhenDataIsValid()
    {
        // Arrange
        var command = _productFixture.CreateProductCommandDTO();

        // Act
        var result = await _productFixture.CreateProductValidator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact(DisplayName = "Must return error when Name value is invalid")]
    public async Task CreateProductValidation_MustReturnError_WhenNameValueIsInvalid()
    {
        // Arrange
        var command = _productFixture.CreateProductCommandDTO(name: "");

        // Act
        var result = await _productFixture.CreateProductValidator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Name");
        Assert.Contains(result.Errors, x => x.ErrorMessage == "Name is required.");
    }
    [Fact(DisplayName = "Must return error when Name exceed 100 caracters")]
    public async Task CreateProductValidation_MustReturnError_WhenNameExceed100Caracters()
    {
        // Arrange
        var name = _productFixture.CreateString(101);
        var command = _productFixture.CreateProductCommandDTO(name: name);

        // Act
        var result = await _productFixture.CreateProductValidator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Name");
        Assert.Contains(result.Errors, x => x.ErrorMessage == "Name cannot exceed 100 characters.");
    }

    [Fact(DisplayName = "Must return error when Description value is invalid")]
    public async Task CreateProductValidation_MustReturnError_WhenDescriptionValueIsInvalid()
    {
        // Arrange
        var command = _productFixture.CreateProductCommandDTO(description: "");

        // Act
        var result = await _productFixture.CreateProductValidator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Description");
        Assert.Contains(result.Errors, x => x.ErrorMessage == "Description is required.");
    }
    [Fact(DisplayName = "Must return error when Description exceed 500 caracters")]
    public async Task CreateProductValidation_MustReturnError_WhenNDescriptionExceed100Caracters()
    {
        // Arrange
        var description = _productFixture.CreateString(501);
        var command = _productFixture.CreateProductCommandDTO(description: description);

        // Act
        var result = await _productFixture.CreateProductValidator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Description");
        Assert.Contains(result.Errors, x => x.ErrorMessage == "Description cannot exceed 500 characters.");
    }

    [Fact(DisplayName = "Must return error when Price value is invalid")]
    public async Task CreateProductValidation_MustReturnError_WhenPriceValueIsInvalid()
    {
        // Arrange
        var command = _productFixture.CreateProductCommandDTO(price: -1);

        // Act
        var result = await _productFixture.CreateProductValidator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Price");
        Assert.Contains(result.Errors, x => x.ErrorMessage == "Price must be greater than zero.");
    }
    
    [Fact(DisplayName = "Must return error when StockQuantity value is invalid")]
    public async Task CreateProductValidation_MustReturnError_WhenStockQuantityValueIsInvalid()
    {
        // Arrange
        var command = _productFixture.CreateProductCommandDTO(stockQuantity: -1);

        // Act
        var result = await _productFixture.CreateProductValidator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "StockQuantity");
        Assert.Contains(result.Errors, x => x.ErrorMessage == "Stock Quantity cannot be negative.");
    }

}