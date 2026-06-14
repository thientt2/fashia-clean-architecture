using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using ValidationException = Fashia.Application.Common.Exceptions.ValidationException;

namespace Fashia.Application.Common.Behaviours;

public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehaviour<TRequest, TResponse>> _logger;

    public ValidationBehaviour(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehaviour<TRequest, TResponse>> logger
    )
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(validator => validator.ValidateAsync(context, cancellationToken))
        );

        var failures = validationResults
            .SelectMany(result => result.Errors)
            .Where(failure => failure is not null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        var errorMessage = string.Join(
            Environment.NewLine,
            failures
                .GroupBy(failure =>
                    string.IsNullOrWhiteSpace(failure.PropertyName)
                        ? typeof(TRequest).Name
                        : failure.PropertyName
                )
                .Select(group =>
                    $"{group.Key}: {string.Join(", ", group.Select(x => x.ErrorMessage).Distinct())}"
                )
        );

        _logger.LogWarning(
            "Validation failed for request {RequestName}. Errors:{NewLine}{ValidationErrors}",
            typeof(TRequest).Name,
            Environment.NewLine,
            errorMessage
        );

        throw new ValidationException(failures);
    }
}
