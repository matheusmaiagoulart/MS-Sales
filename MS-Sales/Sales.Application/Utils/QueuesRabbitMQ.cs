namespace Sales.Application.Utils;

public static class QueuesRabbitMQ
{
    
        // Stock Validation Queues
        public const string REQUEST_VALIDATION_STOCK = "order.validation.stock.request";
        public const string RESPONSE_VALIDATION_STOCK = "order.validation.stock.response";
        
        // Decrease Stock Queues
        public const string REQUEST_DECREASE_STOCK = "order.decrease.stock.request";
        public const string RESPONSE_DECREASE_STOCK = "order.decrease.stock.response";
        

}