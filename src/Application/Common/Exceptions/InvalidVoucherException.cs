namespace Fashia.Application.Common.Exceptions;

public sealed class InvalidVoucherException : Exception
{
    public InvalidVoucherException(string message)
        : base(message)
    {
    }
}
