using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Stock.Application.Products.Commands.CreateProduct;
using Stock.Application.Products.Commands.UpdateProduct;
using Stock.Application.Products.Commands.UpdateStock;
using Stock.Application.Products.Queries.GetProductById;
using Stock.Application.Products.Queries.StockValidation;
using Stock.Domain.Models.Interfaces;
using Stock.Infrastructure.BackgroundServices.ClearReservedStockExpired;
using Stock.Infrastructure.Data.Context;
using Stock.Infrastructure.Middleware;
using Stock.Infrastructure.RabbitMQ.Config;
using Stock.Infrastructure.RabbitMQ.Consumers;
using Stock.Infrastructure.RabbitMQ.Interfaces;
using Stock.Infrastructure.RabbitMQ.Producers;
using Stock.Infrastructure.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Stock API", 
        Version = "v1"
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("MS-Stock-Connection")
    )
);

builder.Services.AddScoped<IValidator<CreateProductCommand>, CreateProductCommandValidator>();
builder.Services.AddScoped<IValidator<UpdateProductCommand>, UpdateProductCommandValidator>();
builder.Services.AddScoped<IValidator<GetProductByIdQuery>, GetProductByIdValidator>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetProductByIdQuery).Assembly));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<StockValidationHandler>();
builder.Services.AddScoped<IGenericConsumer, GenericConsumer>();
builder.Services.AddScoped<IGenericPublisher, GenericProducer>();
builder.Services.AddScoped<UpdateStockCommandHandler>();

builder.Services.AddHostedService<ClearReservedStockExpiredProcess>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Stock API v1");
    });
}

app.UseMiddleware<MiddlewareApplication.ErrorHandleMiddleware>(); // Adiciona o middleware
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using var scope = app.Services.CreateScope();
var stockValidationHandler = scope.ServiceProvider.GetRequiredService<StockValidationHandler>();
var genericConsumer = scope.ServiceProvider.GetRequiredService<IGenericConsumer>();
var genericPublisher = scope.ServiceProvider.GetRequiredService<IGenericPublisher>();
var logger = scope.ServiceProvider.GetRequiredService<ILogger<ValidationStockAvailableConsumer>>();
var consumer = new ValidationStockAvailableConsumer(stockValidationHandler, genericConsumer, genericPublisher, logger);

_ = Task.Run(async () =>
{
    try
    {
        await consumer.Consumer<StockValidationQuery>(QueuesConfig.RabbitMQQueues.REQUEST_VALIDATION_STOCK);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro no consumer: {ex.Message}");
    }
});

using var scope2 = app.Services.CreateScope();
var decreaseStockHandler = scope.ServiceProvider.GetRequiredService<UpdateStockCommandHandler>();
var genericConsumer2 = scope.ServiceProvider.GetRequiredService<IGenericConsumer>();
var genericPublisher2 = scope.ServiceProvider.GetRequiredService<IGenericPublisher>();
var logger2 = scope.ServiceProvider.GetRequiredService<ILogger<DecreaseStockConsumer>>();
var consumer2 = new DecreaseStockConsumer(genericConsumer2, decreaseStockHandler, genericPublisher2, logger2);

_ = Task.Run(async () =>
{
    try
    {
        await consumer2.Consumer<UpdateStockCommand>(QueuesConfig.RabbitMQQueues.REQUEST_DECREASE_STOCK, QueuesConfig.RabbitMQQueues.RESPONSE_DECREASE_STOCK
        );
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro no consumer: {ex.Message}");
    }
});

 app.Run();
