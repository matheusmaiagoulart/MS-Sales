namespace Sales.Infrastructure.RabbitMQConfig;

public class ConfigRabbitMQ
{
    public class RabbitMQSetting
    {
        public string? HostName { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }

    //Nome da fila RabbitMQ 
    public static class RabbitMQQueues
    {
        public const string OrderValidationStockQueue = "orderValidationStockQueue";

    }

}