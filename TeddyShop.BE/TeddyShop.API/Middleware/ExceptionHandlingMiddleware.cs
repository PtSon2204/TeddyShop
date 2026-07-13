using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace TeddyShop.API.Middleware
{
    /// <summary>
    /// Middleware bắt toàn bộ unhandled exception, map sang HTTP status code
    /// và trả về chuẩn RFC 7807 ProblemDetails.
    /// Client không bao giờ thấy stack trace thật.
    /// </summary>
    public class ExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
            => _logger = logger;

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception tại {Path}: {Message}",
                    context.Request.Path, ex.Message);

                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, title, detail) = exception switch
            {
                ArgumentNullException e     => (400, "Bad Request", e.Message),
                ArgumentException e         => (400, "Bad Request", e.Message),
                UnauthorizedAccessException => (401, "Unauthorized", "Bạn không có quyền truy cập."),
                KeyNotFoundException e      => (404, "Not Found", e.Message),
                _                           => (500, "Internal Server Error",
                                                    "Đã xảy ra lỗi. Vui lòng thử lại sau.")
            };

            context.Response.StatusCode  = statusCode;
            context.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status   = statusCode,
                Title    = title,
                Detail   = detail,
                Instance = context.Request.Path
            };

            var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
