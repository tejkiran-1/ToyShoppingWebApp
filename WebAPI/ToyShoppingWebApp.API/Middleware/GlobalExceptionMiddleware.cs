using System.Net;
using System.Text.Json;

namespace ToyShoppingWebApp.API.Middleware
{
    /// <summary>
    /// Global exception middleware that catches all unhandled exceptions
    /// and returns consistent JSON error responses
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Generate correlation ID for tracing this request
            string correlationId = Guid.NewGuid().ToString();
            context.Items["CorrelationId"] = correlationId;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log exception with correlation ID
                _logger.LogError(ex, "Unhandled exception occurred. CorrelationId: {CorrelationId}", correlationId);

                // Set response properties
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // Create consistent error response
                var errorResponse = new
                {
                    message = "An error occurred while processing your request.",
                    correlationId = correlationId,
                    timestamp = DateTime.UtcNow,
                    statusCode = context.Response.StatusCode
                };

                // Add detailed error info in Development only
                if (context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
                {
                    errorResponse = new
                    {
                        message = ex.Message,
                        correlationId = correlationId,
                        timestamp = DateTime.UtcNow,
                        statusCode = context.Response.StatusCode,
                        exception = ex.GetType().Name,
                        stackTrace = ex.StackTrace,
                        innerException = ex.InnerException?.Message
                    };
                }

                // Write error response
                await context.Response.WriteAsJsonAsync(errorResponse);
            }
        }
    }
}
