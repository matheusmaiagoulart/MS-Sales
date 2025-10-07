using System.Text.Json;
using FluentResults;
using MediatR;
using RabbitMQ.Client;
using Stock.Application.Products.Commands.UpdateStock;
using Stock.Infrastructure.RabbitMQ.Interfaces;

namespace Stock.Infrastructure.RabbitMQ.Consumers;

public class DecreaseStockConsumer
{
    private readonly IGenericConsumer _genericConsumer;
    private readonly IGenericPublisher _genericPublisher;
    private readonly IRequestHandler<UpdateStockCommand, Result<UpdateStockCommandResponse>> _handler;

    public DecreaseStockConsumer(IGenericConsumer genericConsumer,
        IRequestHandler<UpdateStockCommand, Result<UpdateStockCommandResponse>> handler,
        IGenericPublisher genericPublisher)
    {
        _handler = handler;
        _genericConsumer = genericConsumer;
        _genericPublisher = genericPublisher;
    }


    public async Task Consumer<T>(string queueName, string queueResponse)
    {
        await _genericConsumer.Consumer<UpdateStockCommand>(queueName, async (message) =>
        {
            var response = new UpdateStockCommandResponse(message.IdOrder, false);
            var basicProperties = new BasicProperties() { ReplyTo = queueResponse };
            Console.WriteLine("Mensagem recebida para diminuir o estoque: " + message.Items.Count + " itens.");
            var result = await _handler.Handle(message, CancellationToken.None);

            if (result.IsFailed)
            {
                //publish na fila de resposta que falhou
                Console.WriteLine("Falha ao diminuir o estoque para o pedido: " + message.IdOrder);
                await ReturnResponseDecreaseStock(response, basicProperties);
            }
            else
            {
                response.IsSaleSuccess = true;
                Console.WriteLine("Estoque diminuído com sucesso para o pedido: " + message.IdOrder);
                //publish na fila de resposta que deu certo
                await ReturnResponseDecreaseStock(response, basicProperties);
            }
        });
    }

    private async Task ReturnResponseDecreaseStock(UpdateStockCommandResponse response, BasicProperties basicProperties)
    {
        _genericPublisher.Publisher(response, basicProperties);
        Console.WriteLine("Mensagem de resposta estoque." + response);
    }
}