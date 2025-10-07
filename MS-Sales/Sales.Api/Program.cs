using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Sales.Application.Orders.Commands.CreateOrder;
using Sales.Application.Services;
using Sales.Domain.Interfaces;
using Sales.Infrastructure.Data.Context;
using Sales.Infrastructure.RabbitMQ.Consumer;
using Sales.Infrastructure.RabbitMQ.Producer;
using Sales.Infrastructure.Repository;
using Sales.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("MS-Sales-Connection")
    )
);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommandHandler).Assembly));
builder.Services.AddScoped<IRabbitMqPublisher, GenericProducer>();
builder.Services.AddScoped<IStockValidationService, StockValidationService>();
builder.Services.AddSingleton<IValidationStockResponseConsumer, ValidationStockResponseConsumer>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IValidator<CreateOrderCommand>, CreateOrderCommandValidator>();
builder.Services.AddScoped<IDecreaseStockResponseConsumer, DecreaseStockResponseConsumer>();


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

app.Run();