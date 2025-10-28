using FluentResults;
using Moq;
using Sales.Domain.DTOs;
using Sales.Tests.Fixtures;

namespace Sales.Tests.Application.Orders.Commands.CreateOrder.Services;

public class StockDecreaseRequestServiceTests : IClassFixture<OrderFixture>
{
    private readonly OrderFixture _orderFixture;
    public StockDecreaseRequestServiceTests(OrderFixture orderFixture)
    {
        _orderFixture = orderFixture;
    }

    [Fact(DisplayName = "Must return successfully when stock decrease is successful")]
    public async Task StockDecreaseRequestService_MustReturnSuccessfully_WhenStockDecreaseIsSuccessful()
    {
        _orderFixture.MockRepository.Reset();
        _orderFixture.MockGenericPublisher.Reset();
        _orderFixture.MockDecreaseStockResponseConsumer.Reset();
        
        // Arrange
        var command = _orderFixture.CreateOrderCommandDTO();
        var idOrder = Guid.NewGuid();

        var updateStockResponse = new UpdateStockResponse(idOrder, true);
        
        _orderFixture.MockDecreaseStockResponseConsumer
            .Setup(x => x.Consume(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(Result.Ok(updateStockResponse));
        
        // Act
        var service = _orderFixture.CreateStockDecreaseRequestService();
        var result = await service.SendDecreaseStockRequest(command, idOrder);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailed);
        Assert.NotNull(result.Value);
        Assert.True(result.Value.IsSaleSuccess);
        _orderFixture.MockGenericPublisher.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Once);
        _orderFixture.MockDecreaseStockResponseConsumer.Verify(x => x.Consume(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<int>()), Times.Once);
    }

    [Fact(DisplayName = "Must return failed when stock decrease consumer fails")]
    public async Task StockDecreaseRequestService_MustReturnFailed_WhenStockDecreaseConsumerFails()
    {
        _orderFixture.MockRepository.Reset();
        _orderFixture.MockGenericPublisher.Reset();
        _orderFixture.MockDecreaseStockResponseConsumer.Reset();
        
        // Arrange
        var command = _orderFixture.CreateOrderCommandDTO();
        var idOrder = Guid.NewGuid();
        var errorMessage = "Consumer failed";
        
        _orderFixture.MockDecreaseStockResponseConsumer
            .Setup(x => x.Consume(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(Result.Fail<UpdateStockResponse>(errorMessage));
        
        // Act
        var service = _orderFixture.CreateStockDecreaseRequestService();
        var result = await service.SendDecreaseStockRequest(command, idOrder);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailed);
        Assert.Contains(errorMessage, result.Errors.Select(e => e.Message));
        _orderFixture.MockGenericPublisher.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Once);
        _orderFixture.MockDecreaseStockResponseConsumer.Verify(x => x.Consume(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<int>()), Times.Once);
    }

    [Fact(DisplayName = "Must return failed when stock decrease is not successful")]
    public async Task StockDecreaseRequestService_MustReturnFailed_WhenStockDecreaseIsNotSuccessful()
    {
        _orderFixture.MockRepository.Reset();
        _orderFixture.MockGenericPublisher.Reset();
        _orderFixture.MockDecreaseStockResponseConsumer.Reset();
        
        // Arrange
        var command = _orderFixture.CreateOrderCommandDTO();
        var idOrder = Guid.NewGuid();

        var updateStockResponse = new UpdateStockResponse(idOrder, false);
        _orderFixture.MockDecreaseStockResponseConsumer
            .Setup(x => x.Consume(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(updateStockResponse);
        
        // Act
        var service = _orderFixture.CreateStockDecreaseRequestService();
        var result = await service.SendDecreaseStockRequest(command, idOrder);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailed);
        Assert.Contains("An error occurred while decreasing stock.", result.Errors.Select(e => e.Message));
        _orderFixture.MockGenericPublisher.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Once);
        _orderFixture.MockDecreaseStockResponseConsumer.Verify(x => x.Consume(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<int>()), Times.Once);
    }
}