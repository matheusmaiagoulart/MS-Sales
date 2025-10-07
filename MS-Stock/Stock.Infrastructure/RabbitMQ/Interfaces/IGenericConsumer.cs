namespace Stock.Infrastructure.RabbitMQ.Interfaces;

public interface IGenericConsumer
{
    Task Consumer<T>(string queueName, Func<T, Task> onMessag);
}