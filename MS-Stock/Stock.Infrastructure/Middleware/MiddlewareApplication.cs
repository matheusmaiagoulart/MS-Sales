using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Stock.Infrastructure.Middleware;

public class MiddlewareApplication
{
    /// <summary>
    /// Middleware to handle exceptions and provide a consistent error response.
    /// </summary>
    public class ErrorHandleMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandleMiddleware> _logger;

        public ErrorHandleMiddleware(RequestDelegate next, ILogger<ErrorHandleMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = ex switch
                {
                    ArgumentException => StatusCodes.Status400BadRequest,
                    KeyNotFoundException => StatusCodes.Status404NotFound,
                    UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                    InvalidOperationException => StatusCodes.Status400BadRequest,
                    _ => StatusCodes.Status500InternalServerError
                };
                var response = new
                { 
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                };
                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }

}