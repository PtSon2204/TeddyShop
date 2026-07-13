using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TeddyShop.Application.Common.Behaviors;

namespace TeddyShop.Application
{
    /// <summary>
    /// Đăng ký tất cả services thuộc Application layer.
    /// Bao gồm: MediatR, AutoMapper, FluentValidation, Pipeline Behaviors.
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var assembly = typeof(DependencyInjection).Assembly;

            // MediatR — CQRS handlers
             services.AddMediatR(cfg =>
                 cfg.RegisterServicesFromAssembly(assembly));

            // AutoMapper 16.x — AddAutoMapper tích hợp sẵn, scan Profile trong assembly
            services.AddAutoMapper(cfg => cfg.AddMaps(assembly));

            // FluentValidation — validators
             services.AddValidatorsFromAssembly(assembly);

            // Pipeline Behaviors (thứ tự quan trọng: Logging → Validation → Caching)
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

            return services;
        }
    }
}
