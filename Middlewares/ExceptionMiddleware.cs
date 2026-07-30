using Drivious.Exceptions;
using Drivious.Responses;
using System.Text.Json;

namespace Drivious.Middlewares
{
    public class ExceptionMiddleware
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
                context.Request.Method, context.Request.Path);

            // The response is already on the wire - nothing left to write.
            if (context.Response.HasStarted) return;

            int statusCode = ex switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                BadRequestException => StatusCodes.Status400BadRequest,
                UnauthorizedException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };

            // Never leak internal exception details to clients in production.
            string message = statusCode == StatusCodes.Status500InternalServerError && !_env.IsDevelopment()
                ? "An unexpected error occurred."
                : ex.Message;

            context.Response.Clear();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new ApiResponse<object>(false, message, null);

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
        }
    }
}
