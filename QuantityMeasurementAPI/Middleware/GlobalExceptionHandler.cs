using System.Net;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using QuantityMeasurementAPI.DTOs.Common;
using QuantityMeasurementAPI.Exceptions;

namespace QuantityMeasurementAPI.Middleware
{
    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
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
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = new ErrorResponse
            {
                Timestamp = DateTime.UtcNow.ToString("o"),
                Path = context.Request.Path
            };

            switch (exception)
            {
                case UnauthorizedAccessException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Error = "Unauthorized";
                    response.Message = "You are not authorized to access this resource";
                    break;
                    
                case ArgumentNullException:  // Moved before ArgumentException
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Error = "Bad Request";
                    response.Message = exception.Message;
                    break;
                    
                case ArgumentException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Error = "Bad Request";
                    response.Message = exception.Message;
                    break;
                    
                case InvalidOperationException:
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    response.Error = "Conflict";
                    response.Message = exception.Message;
                    break;
                    
                case KeyNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Error = "Not Found";
                    response.Message = exception.Message;
                    break;
                    
                case DivideByZeroException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Error = "Bad Request";
                    response.Message = "Division by zero is not allowed";
                    response.Details = exception.Message;
                    break;
                    
                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Error = "Internal Server Error";
                    response.Message = "An error occurred while processing your request";
                    response.Details = exception.Message;
                    break;
            }

            context.Response.StatusCode = response.StatusCode;
            context.Response.ContentType = "application/json";
            
            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
            });
            
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}