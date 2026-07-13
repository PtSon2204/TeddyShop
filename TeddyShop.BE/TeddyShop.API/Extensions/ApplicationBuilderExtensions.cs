using Microsoft.AspNetCore.Builder;
using TeddyShop.API.Middleware;

namespace TeddyShop.API.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Đăng ký ExceptionHandlingMiddleware vào pipeline
        /// </summary>
        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            return app;
        }
    }
}
