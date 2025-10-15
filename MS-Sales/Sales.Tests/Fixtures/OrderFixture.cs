using Bogus;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using Sales.Application.Orders.Commands.CreateOrder;
using Sales.Application.Orders.Queries.GetOrderById;
using Sales.Application.Services;
using Sales.Domain.Interfaces;
using Sales.Domain.Models;

namespace Sales.Tests.Fixtures;

public class OrderFixture : IDisposable
{
    public Mock<IOrderRepository> MockRepository { get; }
    public Mock<IGenericPublisher> MockGenericPublisher { get; }
    public Mock<ILogger<CreateOrderCommandHandler>> MockLogger { get; }
    public Mock<IDecreaseStockResponseConsumer> MockDecreaseStockConsumer { get; }
    public Mock<IValidationStockResponseConsumer> MockValidationStockConsumer { get; }
    public CreateOrderCommandValidator CreateOrderValidator { get; }
    public GetOrderByIdValidator GetOrderByIdValidator { get; }
    public Faker Faker { get; }

    public OrderFixture()
    {
        MockRepository = new Mock<IOrderRepository>();
        MockGenericPublisher = new Mock<IGenericPublisher>();
        MockLogger = new Mock<ILogger<CreateOrderCommandHandler>>();
        MockDecreaseStockConsumer = new Mock<IDecreaseStockResponseConsumer>();
        MockValidationStockConsumer = new Mock<IValidationStockResponseConsumer>();
        CreateOrderValidator = new CreateOrderCommandValidator();
        GetOrderByIdValidator = new GetOrderByIdValidator();
        Faker = new Faker();
    }

    public CreateOrderCommand CreateOrderCommandDTO(List<OrdemItem>? items = null)
    {
        return new CreateOrderCommand
        {
            Items = items ?? new List<OrdemItem>
            {
                new OrdemItem(Guid.NewGuid(), Faker.Random.Int(1, 10)),
                new OrdemItem(Guid.NewGuid(), Faker.Random.Int(1, 10))
            }
        };
    }

    public GetOrderById CreateGetOrderByIdQuery(Guid? idOrder = null)
    {
        return new GetOrderById(idOrder ?? Guid.NewGuid());
    }

    public CreateOrderCommandHandler CreateOrderCommandHandler()
    {
        return new CreateOrderCommandHandler(
            MockLogger.Object,
            CreateOrderValidator,
            MockRepository.Object,
            MockGenericPublisher.Object,
            MockValidationStockConsumer.Object,
            MockDecreaseStockConsumer.Object
        );
    }

    public GetOrderByIdHandler CreateGetOrderByIdHandler()
    {
        return new GetOrderByIdHandler(MockRepository.Object, GetOrderByIdValidator);
    }

    public Order CreateOrder()
    {
        var items = new List<OrdemItem>
        {
            new OrdemItem(Guid.NewGuid(), Faker.Random.Int(1, 10)),
            new OrdemItem(Guid.NewGuid(), Faker.Random.Int(1, 10))
        };
        
        return new Order(Guid.NewGuid(), items, Faker.Random.Decimal(100, 1000));
    }

    public void Dispose()
    {
        // TODO release managed resources here
    }
}