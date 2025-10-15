using Sales.Tests.Fixtures;

namespace Sales.Tests.Application.Orders.Queries.GetOrderById;

public class GetOrderByIdValidatorTests : IClassFixture<OrderFixture>
{
    private OrderFixture _orderFixture;

    public GetOrderByIdValidatorTests(OrderFixture orderFixture)
    {
        _orderFixture = orderFixture;
    }

    [Fact(DisplayName = "Must return error when IdOrder value is empty")]
    public async Task GetOrderByIdValidation_MustReturnError_WhenIdOrderValueIsEmpty()
    {
        // Arrange
        var query = _orderFixture.CreateGetOrderByIdQuery(idOrder: Guid.Empty);

        // Act
        var result = await _orderFixture.GetOrderByIdValidator.ValidateAsync(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "IdOrder");
        Assert.Contains(result.Errors, x => x.ErrorMessage == "IdOrder must be a valid GUID.");
    }

    [Fact(DisplayName = "Must return error when IdOrder is required")]
    public async Task GetOrderByIdValidation_MustReturnError_WhenIdOrderIsRequired()
    {
        // Arrange
        var query = _orderFixture.CreateGetOrderByIdQuery(idOrder: Guid.Empty);

        // Act
        var result = await _orderFixture.GetOrderByIdValidator.ValidateAsync(query);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == "IdOrder");
        Assert.Contains(result.Errors, x => x.ErrorMessage == "IdOrder is required.");
    }

    [Fact(DisplayName = "Must pass validation when IdOrder has valid value")]
    public async Task GetOrderByIdValidation_MustPassValidation_WhenIdOrderHasValidValue()
    {
        // Arrange
        var query = _orderFixture.CreateGetOrderByIdQuery();

        // Act
        var result = await _orderFixture.GetOrderByIdValidator.ValidateAsync(query);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
