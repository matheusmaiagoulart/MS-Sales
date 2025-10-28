using Moq;
using Stock.Tests.Fixtures;

namespace Stock.Tests.Application.Products.Commands.UpdateStock;

public class UpdateStockHandlerTests : IClassFixture<ProductFixture>
{
    private ProductFixture _productFixture;
    public UpdateStockHandlerTests(ProductFixture productFixture)
    {
        _productFixture = productFixture;
    }
    
    [Fact(DisplayName = "Must update a SUCCESSFUL Stock when have no errors")]
    public async Task UpdateStock_MustReturnSucessful_WhenHaveNoErros()
    {
        // Arrange
        var updateStockCommand = _productFixture.UpdateStockCommandDTO(Guid.NewGuid(), Guid.NewGuid(),3);
        
        _productFixture.MockRepository.Setup(x => x.TryReserve(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(true);
        
        _productFixture.MockRepository.Setup(x => x.ConfirmReservation(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act 
        var service = _productFixture.UpdateStockCommandHandler();
        var result = await service.Handle(updateStockCommand, CancellationToken.None);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Errors);
        _productFixture.MockRepository.Verify(x => x.TryReserve(It.IsAny<Guid>(), It.IsAny<Guid>(),It.IsAny<int>()), 
            Times.Exactly(updateStockCommand.Items.Count));
        _productFixture.MockRepository.Verify(x => x.ConfirmReservation(It.IsAny<Guid>(), It.IsAny<Guid>(),It.IsAny<int>()), 
            Times.Exactly(updateStockCommand.Items.Count));
    }
    
    [Fact(DisplayName = "Must return error and not modify product when DecreaseStock fails")]
    public async Task UpdateStock_MustReturnError_WhenDecreaseStockFails()
    {
        _productFixture.MockRepository.Reset();
        // Arrange
        var productId = Guid.NewGuid();
        var updateStockCommand = _productFixture.UpdateStockCommandDTO(productId, Guid.NewGuid(), 3);
        
        _productFixture.MockRepository.Setup(x => x.TryReserve(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ReturnsAsync(true);
        
        _productFixture.MockRepository.Setup(x => x.ConfirmReservation(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception());

        _productFixture.MockRepository.Setup(x =>
            x.CancelReservation(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>())).ReturnsAsync(true);
        
        
        
        // Act
        var service = _productFixture.UpdateStockCommandHandler();
        var result = await service.Handle(updateStockCommand, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailed);
        Assert.NotEmpty(result.Errors);
        
        _productFixture.MockRepository.Verify(x => x.TryReserve(updateStockCommand.IdOrder,updateStockCommand.Items.First().IdProduct, updateStockCommand.Items.First().Quantity),
            Times.Once);
        _productFixture.MockRepository.Verify(x => x.ConfirmReservation(updateStockCommand.IdOrder,updateStockCommand.Items.First().IdProduct, updateStockCommand.Items.First().Quantity),
            Times.Once);
        _productFixture.MockRepository.Verify(x => x.CancelReservation(updateStockCommand.IdOrder,updateStockCommand.Items.First().IdProduct, updateStockCommand.Items.First().Quantity),
            Times.Once);
        
        _productFixture.MockRepository.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);
    }
}