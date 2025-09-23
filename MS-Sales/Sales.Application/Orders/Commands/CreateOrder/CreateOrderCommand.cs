using FluentResults;
using MediatR;
using Sales.Domain.Models;

namespace Sales.Application.Orders.Commands.CreateOrder;

public record CreateOrderCommand(List<OrdemItem> Items) : IRequest<Result<CreateOrderResponse>>;