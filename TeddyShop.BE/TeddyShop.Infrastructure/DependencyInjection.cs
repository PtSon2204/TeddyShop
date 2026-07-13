using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeddyShop.Application.Interfaces;
using TeddyShop.Infrastructure.Identity;

namespace TeddyShop.Infrastructure
{
    /// <summary>
    /// Đăng ký tất cả services thuộc Infrastructure layer.
    /// Bao gồm: EF Core/PostgreSQL, Repositories, UnitOfWork,
    ///          Redis Cache, RabbitMQ, JWT, Cloudinary, Email.
    /// 
    /// LƯU Ý: ExceptionHandlingMiddleware thuộc API layer — đăng ký ở Program.cs,
    ///        không đăng ký ở đây (tránh vi phạm dependency direction).
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // ── EF Core + PostgreSQL ──────────────────────────────────────
            // TODO: dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
            // services.AddDbContext<ApplicationDbContext>(options =>
            //     options.UseNpgsql(
            //         configuration.GetConnectionString("DefaultConnection")));

            // ── Repositories ──────────────────────────────────────────────
            // services.AddScoped<IProductRepository, ProductRepository>();
            // services.AddScoped<IOrderRepository, OrderRepository>();
            // services.AddScoped<IUserRepository, UserRepository>();
            // services.AddScoped<ICategoryRepository, CategoryRepository>();
            // services.AddScoped<ICartRepository, CartRepository>();
            // services.AddScoped<ICouponRepository, CouponRepository>();

            // ── Unit of Work ──────────────────────────────────────────────
            // services.AddScoped<IUnitOfWork, UnitOfWork>();

            // ── Redis Cache ───────────────────────────────────────────────
            // TODO: dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
            // services.AddStackExchangeRedisCache(options =>
            //     options.Configuration = configuration["Redis:ConnectionString"]);
            // services.AddScoped<ICacheService, RedisCacheService>();

            // ── RabbitMQ ──────────────────────────────────────────────────
            // TODO: dotnet add package RabbitMQ.Client
            // services.AddSingleton<IMessagePublisher, RabbitMQPublisher>();

            // ── JWT Token Service ─────────────────────────────────────────
            //services.AddScoped<ITokenService, JwtTokenService>();
            //services.AddSingleton<IPasswordHasher, PasswordHasher>();

            // ── Cloudinary (File Storage) ─────────────────────────────────
            // TODO: dotnet add package CloudinaryDotNet
            // services.Configure<CloudinarySettings>(configuration.GetSection("Cloudinary"));
            // services.AddScoped<IFileStorageService, CloudinaryStorageService>();

            // ── Email (MailKit) ───────────────────────────────────────────
            // TODO: dotnet add package MailKit
            // services.Configure<EmailSettings>(configuration.GetSection("Email"));
            // services.AddScoped<IEmailService, MailKitEmailService>();

            // ── VietQR Payment ────────────────────────────────────────────
            // services.AddHttpClient<IPaymentService, VietQrPaymentService>();

            return services;
        }
    }
}
