using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Enums;

namespace Fashia.Application.Carts.Common;

internal static class CartLookup
{
    internal static async Task<int> GetCurrentCustomerIdAsync(
        IApplicationDbContext context,
        IUser user,
        CancellationToken cancellationToken
    )
    {
        var customerId = await context
            .Customers.Where(x => x.UserId == user.Id)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (customerId == 0)
        {
            throw new UnauthorizedAccessException("Customer account is required.");
        }

        return customerId;
    }

    internal static Task<Domain.Entities.Cart?> GetActiveCartWithItemsAsync(
        IApplicationDbContext context,
        int customerId,
        CancellationToken cancellationToken
    )
    {
        return context
            .Carts.Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.CustomerId == customerId, cancellationToken);
    }
}
