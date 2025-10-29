using System.Collections.Concurrent;
using FluentResults;
using Sales.Application.Services;
using Sales.Application.Utils;
using Sales.Domain.DTOs;

namespace Sales.Infrastructure.RabbitMQ.Consumer;

public class ValidationStockResponseConsumer : IValidationStockResponseConsumer
{
    private readonly ConcurrentDictionary<Guid, RequestCreateOrderValidationResponse> _responsesList = new();
    private readonly IGenericConsumer _genericConsumer;
    public ValidationStockResponseConsumer(IGenericConsumer genericConsumer)
    {
        _genericConsumer = genericConsumer;
        _genericConsumer.Consumer<RequestCreateOrderValidationResponse>(
            QueuesRabbitMQ.RESPONSE_VALIDATION_STOCK, async (result) =>
            {
                _responsesList[result.IdOrder] = result; // Adding or updating the response in the dictionary
            }
        );
    }

    public async Task<Result<RequestCreateOrderValidationResponse>> Consume(string queue, Guid idOrder, int timeoutSeconds)
    {
        var responseDictonary = GetResponseInDictionary(idOrder);
        if (responseDictonary != null)
            return responseDictonary;

        try
        {
            var maxRetry = timeoutSeconds;
            var timeout = 1; // Seconds

            for (int i = 0; i < maxRetry; i++)
            {
                var response = GetResponseInDictionary(idOrder);
                if (response != null)
                {
                    Console.WriteLine($"Message found in list: {response}");
                    return Result.Ok(response);
                }

                if (i < maxRetry - 1)
                    await Task.Delay(TimeSpan.FromSeconds(timeout));
            }
        }
        catch (OperationCanceledException e)
        {
            return Result.Fail<RequestCreateOrderValidationResponse>("Operation canceled: " + e.Message);
        }

        return Result.Fail<RequestCreateOrderValidationResponse>("Timeout: No response received within the specified time.");
    }


    private RequestCreateOrderValidationResponse? GetResponseInDictionary(Guid idOrder)
    {
        if (_responsesList.TryRemove(idOrder, out var response))
            return response;

        return null;
    }
}