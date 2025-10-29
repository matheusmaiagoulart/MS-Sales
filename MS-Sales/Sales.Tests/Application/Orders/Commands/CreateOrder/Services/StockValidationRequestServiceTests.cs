using FluentResults;
using Moq;
using Sales.Domain.DTOs;
using Sales.Tests.Fixtures;

namespace Sales.Tests.Application.Orders.Commands.CreateOrder.Services;

public class StockValidationRequestServiceTests : IClassFixture<OrderFixture>
{
    private readonly OrderFixture _orderFixture;
    public StockValidationRequestServiceTests(OrderFixture orderFixture)
    {
        _orderFixture = orderFixture;
    }
    
    
    [Fact(DisplayName = "Must return a successfully validation")]
    public async Task StockValidationRequestService_MustReturnSuccessfully_WhenValidationReturnAvailable()
    {
        _orderFixture.MockRepository.Reset();
        _orderFixture.MockGenericPublisher.Reset();
        _orderFixture.MockValidationStockResponseConsumer.Reset();
        // Arrange
        var command = _orderFixture.CreateOrderCommandDTO();
        
        var stockValidationResponse = new RequestCreateOrderValidationResponse
        {
            IdOrder = Guid.NewGuid(),
            IsStockAvailable = true,
            ValueAmount = 100m
        };
    
        _orderFixture.MockValidationStockResponseConsumer
            .Setup(x => x.Consume( It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(stockValidationResponse);
        
    
        // Act
        var service = _orderFixture.CreateStockValidationRequestService();
        var result = await service.SendStockValidationRequest(command, command.IdOrder);
    
        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailed);
        Assert.NotNull(result.Value);
        _orderFixture.MockValidationStockResponseConsumer.Verify(x => x.Consume(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<int>()), Times.Once);
    }
    
    [Fact(DisplayName = "Must return failed when stock validation consumer fails")]
    public async Task StockValidationRequestService_MustReturnFailed_WhenStockValidationConsumerFails()
    {
        _orderFixture.MockRepository.Reset();
        _orderFixture.MockGenericPublisher.Reset();
        _orderFixture.MockValidationStockResponseConsumer.Reset();
        // Arrange
        var command = _orderFixture.CreateOrderCommandDTO();
        
        var errorMessage = "Consumer failed";
        _orderFixture.MockValidationStockResponseConsumer
            .Setup(x => x.Consume(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(Result.Fail<RequestCreateOrderValidationResponse>(errorMessage));
        
        // Act
        var service = _orderFixture.CreateStockValidationRequestService();
        var result = await service.SendStockValidationRequest(command, command.IdOrder);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailed);
        Assert.Contains(errorMessage, result.Errors.Select(e => e.Message));
        _orderFixture.MockValidationStockResponseConsumer.Verify(x => x.Consume(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<int>()), Times.Once);
    }
    
    [Fact(DisplayName = "Must return failed when stock is not available")]
    public async Task StockValidationRequestService_MustReturnFailed_WhenStockIsNotAvailable()
    {
        _orderFixture.MockRepository.Reset();
        _orderFixture.MockGenericPublisher.Reset();
        _orderFixture.MockValidationStockResponseConsumer.Reset();
        // Arrange
        var command = _orderFixture.CreateOrderCommandDTO();
        
        var stockValidationResponse = new RequestCreateOrderValidationResponse
        {
            IdOrder = Guid.NewGuid(),
            IsStockAvailable = false,
            ValueAmount = 100m
        };
        
        _orderFixture.MockValidationStockResponseConsumer
            .Setup(x => x.Consume(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(stockValidationResponse);
        
        // Act
        var service = _orderFixture.CreateStockValidationRequestService();
        var result = await service.SendStockValidationRequest(command, command.IdOrder);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailed);
        Assert.Contains("Stock is not available for one or more products in the order.", result.Errors.Select(e => e.Message));
        _orderFixture.MockValidationStockResponseConsumer.Verify(x => x.Consume(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<int>()), Times.Once);
    }
}