using DotNetEnv;
using Serilog;
using TeddyShop.API.Extensions;
using TeddyShop.API.Middleware;
using TeddyShop.Application;
using TeddyShop.Infrastructure;

namespace TeddyShop.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // ── Load .env TRƯỚC TIÊN — trước khi đọc Configuration ────────
            // NoClobber(): không ghi đè biến đã có trong hệ thống
            // → production env vars (Docker/Server) luôn ưu tiên hơn .env
            Env.NoClobber().Load();

            var builder = WebApplication.CreateBuilder(args);

            // ── Serilog — đọc config từ appsettings (đã có env vars) ──────
            builder.Host.UseSerilog((ctx, cfg) =>
                cfg.ReadFrom.Configuration(ctx.Configuration)
                   .Enrich.FromLogContext()
                   .WriteTo.Console()
                   .WriteTo.File(
                       path: "logs/teddyshop-.log",
                       rollingInterval: RollingInterval.Day,
                       retainedFileCountLimit: 30
                   ));

            // ── DI từng layer ─────────────────────────────────────────────
            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);

            // ── API-specific services ─────────────────────────────────────
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSignalR();
            builder.Services.AddSwaggerDocumentation();
            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddCorsPolicy(builder.Configuration);
            builder.Services.AddRateLimiting(builder.Configuration);

            // ExceptionHandlingMiddleware dùng IMiddleware — phải đăng ký DI tường minh
            builder.Services.AddTransient<ExceptionHandlingMiddleware>();

            var app = builder.Build();

            // ── Middleware pipeline (thứ tự rất quan trọng!) ───────────────
            app.UseExceptionHandlingMiddleware();
            app.UseSerilogRequestLogging();
            app.UseHttpsRedirection();
            app.UseCors("AllowFrontend");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRateLimiter();

            app.MapControllers();
            // app.MapHub<NotificationHub>("/hubs/notifications");   // mở khi tạo Hub
            // app.MapHub<OrderTrackingHub>("/hubs/order-tracking"); // mở khi tạo Hub

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TeddyShop API v1");
                    options.RoutePrefix = string.Empty; // Swagger tại root "/"
                });
            }

            app.Run();
        }
    }
}
