using RabbitMQ.Client;

namespace Stock.Infrastructure.RabbitMQ.Interfaces;

public interface IGenericPublisher
{
    Task Publisher<T>(T messageResponse, BasicProperties basicProperties);
}