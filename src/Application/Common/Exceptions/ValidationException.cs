using FluentValidation.Results;

namespace Fashia.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base(BuildErrorMessage(failures))
    {
        Errors = failures
            .GroupBy(
                failure =>
                    string.IsNullOrWhiteSpace(failure.PropertyName)
                        ? "Request"
                        : failure.PropertyName,
                failure => failure.ErrorMessage
            )
            .ToDictionary(
                failureGroup => failureGroup.Key,
                failureGroup => failureGroup.Distinct().ToArray()
            );
    }

    public IDictionary<string, string[]> Errors { get; }

    private static string BuildErrorMessage(IEnumerable<ValidationFailure> failures)
    {
        var failureList = failures.ToList();

        if (failureList.Count == 0)
            return "One or more validation failures have occurred.";

        var errors = failureList
            .GroupBy(failure =>
                string.IsNullOrWhiteSpace(failure.PropertyName) ? "Request" : failure.PropertyName
            )
            .Select(group =>
                $"{group.Key}: {string.Join(", ", group.Select(x => x.ErrorMessage).Distinct())}"
            );

        return "One or more validation failures have occurred."
            + Environment.NewLine
            + string.Join(Environment.NewLine, errors);
    }
}
