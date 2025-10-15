using Sales.Tests.Fixtures;
using Sales.Domain.Models;

namespace Sales.Tests.Application.Orders.Commands.CreateOrder;

public class CreateOrderValidatorTests : IClassFixture<OrderFixture>
{
    private OrderFixture _orderFixture;

    public CreateOrderValidatorTests(OrderFixture orderFixture)
    {
        _orderFixture = orderFixture;
    }

    [Fact(DisplayName = "Must return error when Items value is invalid")]
    public async Task CreateOrderValidation_MustReturnError_WhenItemsValueIsInvalid()
    {
        // Arrange
        var command = _orderFixture.CreateOrderCommandDTO(items: new List<OrdemItem>());

        // Act
        var result = await _orderFixture.CreateOrderValidator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Items");
        Assert.Contains(result.Errors, x => x.ErrorMessage == "Items cannot be null or empty.");
    }

    [Fact(DisplayName = "Must return error when Items is null")]
    public async Task CreateOrderValidation_MustReturnError_WhenItemsIsNull()
    {
        // Arrange
        var command = _orderFixture.CreateOrderCommandDTO(items: new List<OrdemItem>());

        // Act
        var result = await _orderFixture.CreateOrderValidator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "Items");
        Assert.Contains(result.Errors, x => x.ErrorMessage == "Items cannot be null or empty.");
    }

    [Fact(DisplayName = "Must pass validation when Items has valid values")]
    public async Task CreateOrderValidation_MustPassValidation_WhenItemsHasValidValues()
    {
        // Arrange
        var command = _orderFixture.CreateOrderCommandDTO();

        // Act
        var result = await _orderFixture.CreateOrderValidator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
