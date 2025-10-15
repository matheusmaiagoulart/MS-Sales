using Sales.Tests.Fixtures;
using FluentResults;
using Moq;
using Sales.Domain.Models;
using Sales.Domain.DTOs;
using Sales.Application.Orders.Commands.CreateOrder;

namespace Sales.Tests.Application.Orders.Commands.CreateOrder;

public class CreateOrderHandlerTests : IClassFixture<OrderFixture>
{
    private OrderFixture _orderFixture;

    public CreateOrderHandlerTests(OrderFixture orderFixture)
    {
        _orderFixture = orderFixture;
    }
    
    [Fact(DisplayName = "Must return validation errors when command is invalid")]
    public async Task CreateOrder_MustReturnValidationErrors_WhenCommandIsInvalid()
    {
        _orderFixture.MockRepository.Reset();
        // Arrange
        var invalidCommand = _orderFixture.CreateOrderCommandDTO(items: new List<OrdemItem>());

        // Act
        var service = _orderFixture.CreateOrderCommandHandler();
        var result = await service.Handle(invalidCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.NotEmpty(result.Errors);
        _orderFixture.MockRepository.Verify(x => x.CreateOrder(It.IsAny<Order>()), Times.Never);
        _orderFixture.MockRepository.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact(DisplayName = "Must return Network error when database is not reachable")]
    public async Task CreateOrder_MustReturnError_WhenDatabaseIsNotReachable()
    {
        _orderFixture.MockRepository.Reset();
        // Arrange
        var command = _orderFixture.CreateOrderCommandDTO();
        
        var stockValidationResponse = new RequestCreateOrderValidationResponse
        {
            IdOrder = Guid.NewGuid(),
            IsStockAvailable = true,
            ValueAmount = 100m
        };
        
        var decreaseStockResponse = new UpdateStockResponse(Guid.NewGuid(), true);

        _orderFixture.MockValidationStockConsumer
            .Setup(x => x.Consume(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(Result.Ok(stockValidationResponse));

        _orderFixture.MockDecreaseStockConsumer
            .Setup(x => x.Consume(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(Result.Ok(decreaseStockResponse));

        _orderFixture.MockRepository.Setup(x => x.SaveChangesAsync())
            .Throws(new Exception());

        // Act
        var service = _orderFixture.CreateOrderCommandHandler();
        var result = await service.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.False(result.IsSuccess);
        Assert.NotEmpty(result.Errors);
        _orderFixture.MockRepository.Verify(x => x.CreateOrder(It.IsAny<Order>()), Times.Once);
        _orderFixture.MockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact(DisplayName = "Must return error when stock validation fails")]
    public async Task CreateOrder_MustReturnError_WhenStockValidationFails()
    {
        _orderFixture.MockRepository.Reset();
        // Arrange
        var command = _orderFixture.CreateOrderCommandDTO();

        _orderFixture.MockValidationStockConsumer
            .Setup(x => x.Consume(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(Result.Fail("Stock validation failed"));

        // Act
        var service = _orderFixture.CreateOrderCommandHandler();
        var result = await service.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.NotEmpty(result.Errors);
        _orderFixture.MockRepository.Verify(x => x.CreateOrder(It.IsAny<Order>()), Times.Never);
        _orderFixture.MockRepository.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact(DisplayName = "Must return error when stock is not available")]
    public async Task CreateOrder_MustReturnError_WhenStockIsNotAvailable()
    {
        _orderFixture.MockRepository.Reset();
        // Arrange
        var command = _orderFixture.CreateOrderCommandDTO();
        
        var stockValidationResponse = new RequestCreateOrderValidationResponse
        {
            IdOrder = Guid.NewGuid(),
            IsStockAvailable = false,
            ValueAmount = 100m
        };

        _orderFixture.MockValidationStockConsumer
            .Setup(x => x.Consume(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(Result.Ok(stockValidationResponse));

        // Act
        var service = _orderFixture.CreateOrderCommandHandler();
        var result = await service.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Contains(result.Errors, e => e.Message.Contains("Stock is not available"));
        _orderFixture.MockRepository.Verify(x => x.CreateOrder(It.IsAny<Order>()), Times.Never);
        _orderFixture.MockRepository.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact(DisplayName = "Must return error when stock decrease fails")]
    public async Task CreateOrder_MustReturnError_WhenStockDecreaseFails()
    {
        _orderFixture.MockRepository.Reset();
        // Arrange
        var command = _orderFixture.CreateOrderCommandDTO();
        
        var stockValidationResponse = new RequestCreateOrderValidationResponse
        {
            IdOrder = Guid.NewGuid(),
            IsStockAvailable = true,
            ValueAmount = 100m
        };

        _orderFixture.MockValidationStockConsumer
            .Setup(x => x.Consume(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(Result.Ok(stockValidationResponse));

        _orderFixture.MockDecreaseStockConsumer
            .Setup(x => x.Consume(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(Result.Fail("Stock decrease failed"));

        // Act
        var service = _orderFixture.CreateOrderCommandHandler();
        var result = await service.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.NotEmpty(result.Errors);
        _orderFixture.MockRepository.Verify(x => x.CreateOrder(It.IsAny<Order>()), Times.Never);
        _orderFixture.MockRepository.Verify(x => x.SaveChangesAsync(), Times.Never);
    }

    [Fact(DisplayName = "Must create order successfully when all validations pass")]
    public async Task CreateOrder_MustCreateOrderSuccessfully_WhenAllValidationsPass()
    {
        _orderFixture.MockRepository.Reset();
        _orderFixture.MockGenericPublisher.Reset();
        // Arrange
        var command = _orderFixture.CreateOrderCommandDTO();
        
        var stockValidationResponse = new RequestCreateOrderValidationResponse
        {
            IdOrder = Guid.NewGuid(),
            IsStockAvailable = true,
            ValueAmount = 100m
        };
        
        var decreaseStockResponse = new UpdateStockResponse(Guid.NewGuid(), true);

        _orderFixture.MockValidationStockConsumer
            .Setup(x => x.Consume(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(Result.Ok(stockValidationResponse));

        _orderFixture.MockDecreaseStockConsumer
            .Setup(x => x.Consume(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(Result.Ok(decreaseStockResponse));

        // Act
        var service = _orderFixture.CreateOrderCommandHandler();
        var result = await service.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailed);
        Assert.NotNull(result.Value);
        _orderFixture.MockRepository.Verify(x => x.CreateOrder(It.IsAny<Order>()), Times.Once);
        _orderFixture.MockRepository.Verify(x => x.SaveChangesAsync(), Times.Once);
        _orderFixture.MockGenericPublisher.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Exactly(2));
    }
}