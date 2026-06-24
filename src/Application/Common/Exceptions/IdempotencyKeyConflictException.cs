namespace Fashia.Application.Common.Exceptions;

public sealed class IdempotencyKeyConflictException : Exception
{
    public IdempotencyKeyConflictException(string message)
        : base(message)
    {
    }
}

public sealed class DuplicateRequestInProgressException : Exception
{
    public DuplicateRequestInProgressException(string message)
        : base(message)
    {
    }
}
