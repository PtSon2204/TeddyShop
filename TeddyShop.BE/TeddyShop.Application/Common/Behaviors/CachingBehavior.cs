using MediatR;
using Microsoft.Extensions.Logging;

namespace TeddyShop.Application.Common.Behaviors
{
    /// <summary>
    /// Pipeline Behavior: Cache kết quả của các Query (không cache Command).
    /// Chỉ request implement ICacheableQuery mới được cache.
    /// Chạy CUỐI CÙNG trong pipeline — sau Logging và Validation.
    /// </summary>
    public class CachingBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

        public CachingBehavior(ILogger<CachingBehavior<TRequest, TResponse>> logger)
            => _logger = logger;

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // Chỉ cache nếu request implement ICacheableQuery
            // TODO: Uncomment sau khi tạo ICacheableQuery và inject ICacheService
            // if (request is ICacheableQuery cacheableQuery)
            // {
            //     var cached = await _cacheService.GetAsync<TResponse>(cacheableQuery.CacheKey);
            //     if (cached != null)
            //     {
            //         _logger.LogInformation("[CACHE HIT] {Key}", cacheableQuery.CacheKey);
            //         return cached;
            //     }
            //     var response = await next();
            //     await _cacheService.SetAsync(cacheableQuery.CacheKey, response, cacheableQuery.CacheDuration);
            //     return response;
            // }

            return await next();
        }
    }
}
