using Moq;
using Stock.Tests.Fixtures;

namespace Stock.Tests.Application.Products.Commands.UpdateProduct;

public class UpdateProductsValidationsTestes : IClassFixture<ProductFixture>
{
    private ProductFixture _productFixture;
    public UpdateProductsValidationsTestes(ProductFixture productFixture)
    {
        _productFixture = productFixture;
    }
    
    [Fact(DisplayName = "Must return success when Data is valid")]
    public async Task UpdateProductValidation_MustReturnSucess_WhenDataIsValid()
    {
        // Arrange
        var command = _productFixture.UpdateProductCommandDTO();

        // Act
        var result = await _productFixture.UpdateProductCommandValidator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
    }
    
    [Fact(DisplayName = "Must return error when IdProduct is empty or null")]
    public async Task UpdateProductValidation_MustReturnError_WhenIdProductIsEmpty()
    {
        _productFixture.MockRepository.Reset();
        // Arrange
        var command = _productFixture.UpdateProductCommandDTO(idProduct: Guid.Empty);

        // Act
        var result = await _productFixture.UpdateProductCommandValidator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Product ID is required.");
    }
    
    [Fact(DisplayName = "Must return error when Name exceeds maximum length")]
    public async Task UpdateProductValidation_MustReturnError_WhenNameExceedsMaximumLength()
    {
        _productFixture.MockRepository.Reset();
        // Arrange
        var command = _productFixture.UpdateProductCommandDTO(name: _productFixture.CreateString(101));

        // Act
        var result = await _productFixture.UpdateProductCommandValidator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Name cannot exceed 100 characters.");
    }
    
    [Fact(DisplayName = "Must return error when Description exceeds maximum length")]
    public async Task UpdateProductValidation_MustReturnError_WhenDescriptionExceedsMaximumLength()
    {
        _productFixture.MockRepository.Reset();
        // Arrange
        var command = _productFixture.UpdateProductCommandDTO(description: _productFixture.CreateString(501));

        // Act
        var result = await _productFixture.UpdateProductCommandValidator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Description cannot exceed 500 characters.");
    }
    
    [Fact(DisplayName = "Must return error when Price is less than zero")]
    public async Task UpdateProductValidation_MustReturnError_WhenPriceIsLessThanZero()
    {
        _productFixture.MockRepository.Reset();
        // Arrange
        var command = _productFixture.UpdateProductCommandDTO(price: -1);

        // Act
        var result = await _productFixture.UpdateProductCommandValidator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Price");
        Assert.Contains(result.Errors, e => e.ErrorMessage == "Price must be greater than zero.");
    }
}