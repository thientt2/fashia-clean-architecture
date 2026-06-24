namespace Fashia.Application.Common.Exceptions;

public class CheckoutConflictException : Exception
{
    protected CheckoutConflictException(string message)
        : base(message)
    {
    }
}

public sealed class InsufficientStockException : CheckoutConflictException
{
    public InsufficientStockException(string message)
        : base(message)
    {
    }
}

public sealed class NoFulfillableBranchException : CheckoutConflictException
{
    public NoFulfillableBranchException(string message)
        : base(message)
    {
    }
}

public sealed class ProductUnavailableException : CheckoutConflictException
{
    public ProductUnavailableException(string message)
        : base(message)
    {
    }
}
