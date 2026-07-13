using FluentValidation;
using MediatR;

namespace TeddyShop.Application.Common.Behaviors
{
    /// <summary>
    /// Pipeline Behavior: Chạy tất cả FluentValidation validators trước khi handler xử lý.
    /// Nếu có lỗi validation → throw ValidationException, handler không được gọi.
    /// Chạy SAU LoggingBehavior, TRƯỚC CachingBehavior.
    /// </summary>
    public class ValidationBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
            => _validators = validators;

        // ⚠️ MediatR 12: thứ tự tham số là (request, next, cancellationToken)
        // MediatR 8 cũ là (request, cancellationToken, next) — đã thay đổi
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (!_validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);

            var failures = _validators
                .Select(v => v.Validate(context))
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
                throw new ValidationException(failures);

            return await next();
        }
    }
}
