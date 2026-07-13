using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Threading.RateLimiting;

namespace TeddyShop.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Cấu hình JWT Bearer Authentication + hỗ trợ SignalR token qua query string
        /// </summary>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]
                ?? throw new InvalidOperationException("JwtSettings:SecretKey chưa được cấu hình.");

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer           = true,
                        ValidateAudience         = true,
                        ValidateLifetime         = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer              = jwtSettings["Issuer"],
                        ValidAudience            = jwtSettings["Audience"],
                        IssuerSigningKey         = new SymmetricSecurityKey(
                                                       Encoding.UTF8.GetBytes(secretKey)),
                        ClockSkew                = TimeSpan.Zero
                    };

                    // SignalR gửi token qua query string ?access_token=...
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = ctx =>
                        {
                            var accessToken = ctx.Request.Query["access_token"];
                            var path = ctx.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                path.StartsWithSegments("/hubs"))
                            {
                                ctx.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization();
            return services;
        }

        /// <summary>
        /// Cấu hình Swagger UI có hỗ trợ JWT Bearer
        /// </summary>
        public static IServiceCollection AddSwaggerDocumentation(
            this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title       = "TeddyShop API",
                    Version     = "v1",
                    Description = "API cho hệ thống e-commerce bán thú nhồi bông"
                });

                var jwtScheme = new OpenApiSecurityScheme
                {
                    BearerFormat = "JWT",
                    Name         = "Authorization",
                    In           = ParameterLocation.Header,
                    Type         = SecuritySchemeType.Http,
                    Scheme       = JwtBearerDefaults.AuthenticationScheme,
                    Description  = "Nhập JWT token vào đây. Ví dụ: Bearer eyJhbGci...",
                    Reference    = new OpenApiReference
                    {
                        Id   = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };

                options.AddSecurityDefinition(jwtScheme.Reference.Id, jwtScheme);
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { jwtScheme, Array.Empty<string>() }
                });
            });

            return services;
        }

        /// <summary>
        /// Cấu hình CORS — cho phép FE (React) gọi API và dùng SignalR
        /// </summary>
        public static IServiceCollection AddCorsPolicy(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var allowedOrigins = configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? ["http://localhost:5173"];

            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials(); // bắt buộc cho SignalR WebSocket
                });
            });

            return services;
        }

        /// <summary>
        /// Cấu hình Rate Limiting — giới hạn request theo IP để chống spam/DDoS
        /// </summary>
        public static IServiceCollection AddRateLimiting(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddRateLimiter(options =>
            {
                // Policy mặc định: 100 request/phút mỗi IP
                options.AddFixedWindowLimiter("fixed", limiter =>
                {
                    limiter.PermitLimit          = 200;
                    limiter.Window               = TimeSpan.FromMinutes(1);
                    limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiter.QueueLimit           = 10;
                });

                // Trả về 429 Too Many Requests khi vượt limit
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });

            return services;
        }
    }
}
