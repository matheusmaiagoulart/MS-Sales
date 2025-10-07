namespace Sales.Application.Services;

public interface IRabbitMqPublisher
{
    Task Publish<T>(T order, string queuePub, string? queueResponse, Guid idOrder);
}