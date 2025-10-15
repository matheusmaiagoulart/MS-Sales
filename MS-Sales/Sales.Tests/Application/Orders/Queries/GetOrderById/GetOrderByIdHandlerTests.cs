using Sales.Tests.Fixtures;
using Moq;
using Sales.Domain.Models;

namespace Sales.Tests.Application.Orders.Queries.GetOrderById;

public class GetOrderByIdHandlerTests : IClassFixture<OrderFixture>
{
    private OrderFixture _orderFixture;

    public GetOrderByIdHandlerTests(OrderFixture orderFixture)
    {
        _orderFixture = orderFixture;
    }

    [Fact(DisplayName = "Must return validation errors when query is invalid")]
    public async Task GetOrderById_MustReturnValidationErrors_WhenQueryIsInvalid()
    {
        _orderFixture.MockRepository.Reset();
        // Arrange
        var invalidQuery = _orderFixture.CreateGetOrderByIdQuery(idOrder: Guid.Empty);

        // Act
        var service = _orderFixture.CreateGetOrderByIdHandler();
        var result = await service.Handle(invalidQuery, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.NotEmpty(result.Errors);
        _orderFixture.MockRepository.Verify(x => x.GetOrderById(It.IsAny<Guid>()), Times.Never);
    }

    [Fact(DisplayName = "Must return error when order is not found")]
    public async Task GetOrderById_MustReturnError_WhenOrderIsNotFound()
    {
        _orderFixture.MockRepository.Reset();
        // Arrange
        var query = _orderFixture.CreateGetOrderByIdQuery();
        
        _orderFixture.MockRepository.Setup(x => x.GetOrderById(It.IsAny<Guid>()))
            .ReturnsAsync((Order?)null);

        // Act
        var service = _orderFixture.CreateGetOrderByIdHandler();
        var result = await service.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, e => e.Message == "Order not found");
        _orderFixture.MockRepository.Verify(x => x.GetOrderById(It.IsAny<Guid>()), Times.Once);
    }

    [Fact(DisplayName = "Must return Network error when database is not reachable")]
    public async Task GetOrderById_MustReturnError_WhenDatabaseIsNotReachable()
    {
        _orderFixture.MockRepository.Reset();
        // Arrange
        var query = _orderFixture.CreateGetOrderByIdQuery();
        
        _orderFixture.MockRepository.Setup(x => x.GetOrderById(It.IsAny<Guid>()))
            .Throws(new Exception());

        // Act
        var service = _orderFixture.CreateGetOrderByIdHandler();
        var result = await service.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.False(result.IsSuccess);
        _orderFixture.MockRepository.Verify(x => x.GetOrderById(It.IsAny<Guid>()), Times.Once);
    }

    [Fact(DisplayName = "Must return order successfully when order exists")]
    public async Task GetOrderById_MustReturnOrderSuccessfully_WhenOrderExists()
    {
        _orderFixture.MockRepository.Reset();
        // Arrange
        var query = _orderFixture.CreateGetOrderByIdQuery();
        var expectedOrder = _orderFixture.CreateOrder();
        
        _orderFixture.MockRepository.Setup(x => x.GetOrderById(It.IsAny<Guid>()))
            .ReturnsAsync(expectedOrder);

        // Act
        var service = _orderFixture.CreateGetOrderByIdHandler();
        var result = await service.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailed);
        Assert.NotNull(result.Value);
        Assert.Equal(expectedOrder.IdSale, result.Value.IdSale);
        _orderFixture.MockRepository.Verify(x => x.GetOrderById(It.IsAny<Guid>()), Times.Once);
    }
}
