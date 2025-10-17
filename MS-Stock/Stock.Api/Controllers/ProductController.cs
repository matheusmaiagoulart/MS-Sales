using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Stock.Application.Products.Commands.CreateProduct;
using Stock.Application.Products.Commands.UpdateProduct;
using Stock.Application.Products.Commands.UpdateStock;
using Stock.Application.Products.Queries.GetAllProducts;
using Stock.Application.Products.Queries.GetProductById;

namespace Stock.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;
    public ProductController(IMediator mediator)
    {
        _mediator = mediator;
    }


    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status201Created)]    
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateProduct(CreateProductCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            if (result.IsFailed)
                return BadRequest(result.Errors.Select(e => e.Message));

            return Created("", result.Value);
        }
        catch (Exception e)
        {
            if (e is SqlException)
            {
                return StatusCode(500, "A network error occurred while trying to connect to the database");
            }
            return StatusCode(500, "An error occurred while processing your request: " + e.Message);
        }
    }

    [HttpPatch("updateProduct")]
    [ProducesResponseType(StatusCodes.Status200OK)]    
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProduct(UpdateProductCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            if (result.IsFailed)
                return BadRequest(result.Errors.Select(e => e.Message));

            return Ok(result.Value);
        }
        catch (Exception e)
        {
            return StatusCode(500, "An error occurred while updating the product" + e.Message);
        }
    }

    [HttpPut("updateStock")]
    [ProducesResponseType(StatusCodes.Status200OK)]    
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateStock(UpdateStockCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            if (result.IsFailed)
                return BadRequest(result.Errors.Select(e => e.Message));

            return Ok(result.IsSuccess);
        }
        catch (Exception e)
        {
            return StatusCode(500, "An error occurred while updating the Stock" + e.Message);
        }
    }
    
    [HttpGet("GetProductById/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]    
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProductById( Guid id)
    {
        try
        {
            var command = new GetProductByIdQuery(id);
            var result = await _mediator.Send(command);
            if (result.IsFailed)
                return BadRequest(result.Errors.Select(e => e.Message));

            return Ok(result.Value);
        }
        catch (Exception e)
        {
            return StatusCode(500, "An error occurred while getting the product" + e.Message);
        }
    }
    
    [HttpGet("GetAllProducts")]
    [ProducesResponseType(StatusCodes.Status200OK)]    
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllProduct()
    {
        try
        {
            var command = new GetAllProductsQuery();
            var result = await _mediator.Send(command);
            if (result.IsFailed)
                return BadRequest(result.Errors.Select(e => e.Message));
    
            return Ok(result.Value);
        }
        catch (Exception e)
        {
            return StatusCode(500, "An error occurred while getting the products" + e.Message);
        }
    }
    
}