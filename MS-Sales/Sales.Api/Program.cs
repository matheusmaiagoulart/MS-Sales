using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Sales.Application.Orders.Commands.CreateOrder;
using Sales.Domain.Interfaces;
using Sales.Infrastructure.Data.Context;
using Sales.Infrastructure.RabbitMQConfig;
using Sales.Infrastructure.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Conexao com o banco de dados
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("MS-Sales-Connection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MS-Sales-Connection"))
    )
);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommandHandler).Assembly));
builder.Services.AddScoped<RabbitMQPublisher>();
builder.Services.AddScoped<ConfigRabbitMQ>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IValidator<CreateOrderCommand>, CreateOrderCommandValidator>();

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