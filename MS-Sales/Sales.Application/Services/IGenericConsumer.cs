namespace Sales.Application.Services;

public interface IGenericConsumer
{
    Task Consumer<T>(string queueName, Func<T, Task> onMessag);
}