using Bogus;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using Stock.Application.Products.Commands.CreateProduct;
using Stock.Application.Products.Commands.UpdateProduct;
using Stock.Application.Products.Commands.UpdateStock;
using Stock.Application.Products.Queries.GetAllProducts;
using Stock.Application.Products.Queries.GetProductById;
using Stock.Application.Products.Queries.StockValidation;
using Stock.Domain.Interfaces;
using Stock.Domain.Models;

namespace Stock.Tests.Fixtures;

public class ProductFixture : IDisposable
{
    
    public Mock<IProductRepository> MockRepository { get; }
    public IValidator<CreateProductCommand> CreateProductValidator { get; }
    public IValidator<UpdateProductCommand> UpdateProductValidator { get; }
    public IValidator<UpdateStockCommand> UpdateStockCommandValidator { get; }
    public IValidator<UpdateProductCommand> UpdateProductCommandValidator { get; }
    public IValidator<GetProductByIdQuery> GetProductByIdValidator { get; }
    public IValidator<StockValidationQuery> StockValidationValidator { get; }
    
    public ILogger<UpdateStockCommandHandler> Logger { get; }
    
    
    public Faker Faker { get; }
    public ProductFixture()
    {
        MockRepository = new Mock<IProductRepository>();
        Faker = new Faker();
        CreateProductValidator = new CreateProductCommandValidator();
        UpdateProductCommandValidator = new UpdateProductCommandValidator();
        UpdateProductValidator = new UpdateProductCommandValidator();
        UpdateStockCommandValidator = new UpdateStockCommandValidator();
        GetProductByIdValidator = new GetProductByIdValidator();
        StockValidationValidator = new StockValidationValidator();
        Logger = new Mock<ILogger<UpdateStockCommandHandler>>().Object;
    }
    // Handlers
    public CreateProductCommandHandler CreateProductCommandHandler() { return new CreateProductCommandHandler(MockRepository.Object, CreateProductValidator); }
    public UpdateProductCommandHandler UpdateProductCommandHandler() { return new UpdateProductCommandHandler(MockRepository.Object, UpdateProductCommandValidator); }
    public UpdateStockCommandHandler UpdateStockCommandHandler() { return new UpdateStockCommandHandler(MockRepository.Object, Logger); }
    
    // Query Handlers
    public GetAllProductsHandler GetAllProductsHandler() { return new GetAllProductsHandler(MockRepository.Object); }
    public GetProductByIdHandler GetProductByIdHandler() { return new GetProductByIdHandler(GetProductByIdValidator, MockRepository.Object); }
    public StockValidationHandler StockValidationHandler() { return new StockValidationHandler(MockRepository.Object); }

    // Query DTOs
    public GetAllProductsQuery GetAllProductsQuery()
    {
        return new GetAllProductsQuery();
    }
    
    public GetProductByIdQuery GetProductByIdQuery(Guid? id = null)
    {
        return new GetProductByIdQuery(id ?? Faker.Random.Guid());
    }
    
    public StockValidationQuery StockValidationQuery(Guid? idOrder = null, List<OrderItemDTO>? items = null)
    {
        var orderItems = items ?? new List<OrderItemDTO>
        {
            new OrderItemDTO
            {
                IdProduct = Faker.Random.Guid(),
                Quantity = Faker.Random.Int(1, 100)
            }
        };
        
        return new StockValidationQuery(idOrder ?? Faker.Random.Guid(), orderItems);
    }

    public CreateProductCommand CreateProductCommandDTO(string? name = null, string? description = null, decimal? price = null, int? stockQuantity = null)
    {
        return new CreateProductCommand(
            name: name ?? Faker.Random.String(1,10),
            description: description ?? Faker.Lorem.Sentence(),
            price: price ?? Faker.Random.Decimal(1,1000),
            stockQuantity: stockQuantity ?? Faker.Random.Int(1,100));
    }
    
    public UpdateProductCommand UpdateProductCommandDTO(Guid? idProduct = null, string? name = null, string? description = null, decimal? price = null, int? stockQuantity = null)
    {
        return new UpdateProductCommand(
            id: idProduct ?? Faker.Random.Guid(),
            name: name ?? Faker.Random.String(1,10),
            description: description ?? Faker.Lorem.Sentence(),
            price: price ?? Faker.Random.Decimal(1,1000),
            stockQuantity: stockQuantity ?? Faker.Random.Int(1,100));
    }

    public UpdateStockCommand UpdateStockCommandDTO(Guid? IdOrder = null, Guid? idProduct = null,
        int? stockQuantity = null, int? countItems = null)
    {
        var items = new List<OrderItemDTO>();
        
        var itemCount = countItems ?? 1;

        if (itemCount > 0)
        {
            for (int i = 0; i < itemCount; i++)
            {
                items.Add(new OrderItemDTO
                {
                    IdProduct = idProduct ?? Faker.Random.Guid(),
                    Quantity = stockQuantity ?? Faker.Random.Int(1, 100)
                });
            }
        }

        return new UpdateStockCommand(
            idOrder: IdOrder ?? Faker.Random.Guid(),
            items: items
        );
    }

    public Product CreateProductModel(string? name = null, string? description = null, decimal? price = null, int? stockQuantity = null)
    {
        return new Product(
            name: name ?? Faker.Random.String(1,10),
            description: description ?? Faker.Lorem.Sentence(),
            price: price ?? Faker.Random.Decimal(1,1000),
            stockQuantity: stockQuantity ?? Faker.Random.Int(1,100));
    }
    public string CreateString(int length)
    {
        return Faker.Random.String(length);
    }
    
    public void Dispose()
    {
        // TODO release managed resources here
    }
}
