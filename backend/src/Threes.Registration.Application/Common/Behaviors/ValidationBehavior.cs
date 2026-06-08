using FluentValidation;
using MediatR;
using ValidationException = Threes.Registration.Application.Common.Exceptions.ValidationException;

namespace Threes.Registration.Application.Common.Behaviors;

// runs every fluentvalidation validator registered for the incoming request
// before the handler sees it. if anything fails we throw our own
// ValidationException (grouped by field) which the api renders as a 400. the
// cancellation token is threaded through so async rules can bail out.
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
        {
            var errors = failures
                .GroupBy(f => f.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(f => f.ErrorMessage).Distinct().ToArray());

            throw new ValidationException(errors);
        }

        return await next();
    }
}
