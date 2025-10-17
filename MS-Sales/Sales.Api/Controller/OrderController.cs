using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sales.Application.Orders.Commands.CreateOrder;
using Sales.Application.Orders.Queries.GetOrderById;

namespace Sales.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IMediator _mediator;
    public OrderController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost("CreateOrder")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        
        return BadRequest(result.Errors.Select(x => x.Message));
    }

    [HttpGet("{idOrder:guid}")]
    public async Task<IActionResult> GetOrderById([FromRoute] Guid idOrder)
    {
        var command = new GetOrderById(idOrder);

        var result = await _mediator.Send(command);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        
        return BadRequest(result.Errors.Select(x => x.Message));
    }
    
    
}