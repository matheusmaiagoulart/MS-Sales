namespace Sales.Application.Services;

public interface IGenericPublisher
{
    Task Publish<T>(T order, string queuePub, string? queueResponse, Guid idOrder);
}