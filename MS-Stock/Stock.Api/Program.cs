using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Stock.Application.Products.Commands.CreateProduct;
using Stock.Application.Products.Commands.UpdateProduct;
using Stock.Application.Products.Queries.GetProductById;
using Stock.Application.Products.Queries.StockValidation;
using Stock.Domain.Interfaces;
using Stock.Infrastructure.Data.Context;
using Stock.Infrastructure.RabbitMQ.Consumers;
using Stock.Infrastructure.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Conexao com o banco de dados
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("MS-Stock-Connection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MS-Stock-Connection"))
    )
);

builder.Services.AddScoped<IValidator<CreateProductCommand>, CreateProductCommandValidator>();
builder.Services.AddScoped<IValidator<UpdateProductCommand>, UpdateProductCommandValidator>();
builder.Services.AddScoped<IValidator<GetProductByIdQuery>, GetProductByIdValidator>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetProductByIdQuery).Assembly));
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<StockValidationHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


using var scope = app.Services.CreateScope();
var stockValidationHandler = scope.ServiceProvider.GetRequiredService<StockValidationHandler>();
var consumer = new ValidationStockAvailableConsumer(stockValidationHandler);

_ = Task.Run(async () => await consumer.Consumer<string>());
app.Run();
