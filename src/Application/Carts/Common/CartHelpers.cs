using Fashia.Application.Carts.Queries;
using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Entities;
using Fashia.Domain.Enums;

namespace Fashia.Application.Carts.Common;

internal static class CartHelpers
{
    internal static async Task<int> ResolveCustomerIdAsync(
        IApplicationDbContext context,
        IUser user,
        CancellationToken cancellationToken
    )
    {
        return await GetCurrentCustomerIdAsync(context, user, cancellationToken)
            ?? throw new UnauthorizedAccessException("Authentication cookie is required.");
    }

    internal static async Task<int?> GetCurrentCustomerIdAsync(
        IApplicationDbContext context,
        IUser user,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(user.Id))
            return null;

        return await context
            .Customers.Where(x => x.UserId == user.Id)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    internal static async Task<Cart?> FindCartAsync(
        IApplicationDbContext context,
        int customerId,
        bool createIfMissing,
        CancellationToken cancellationToken
    )
    {
        var cart = await context
            .Carts.Include(x => x.Items)
            .Where(x => x.Status == CartStatus.Active)
            .FirstOrDefaultAsync(x => x.CustomerId == customerId, cancellationToken);

        if (cart is not null || !createIfMissing)
            return cart;

        cart = new Cart(customerId);
        context.Carts.Add(cart);

        return cart;
    }

    internal static async Task ValidateProductVariantAsync(
        IApplicationDbContext context,
        int productVariantId,
        CancellationToken cancellationToken
    )
    {
        var variantExists = await context.ProductVariants.AnyAsync(
            x => x.Id == productVariantId,
            cancellationToken
        );

        if (!variantExists)
            throw new InvalidOperationException("Product variant not found.");
    }

    internal static async Task<CartDto?> ProjectCartAsync(
        IApplicationDbContext context,
        int cartId,
        CancellationToken cancellationToken
    )
    {
        return await context
            .Carts.AsNoTracking()
            .Where(x => x.Id == cartId)
            .Select(x => new CartDto
            {
                Id = x.Id,
                CustomerId = x.CustomerId,
                Status = x.Status.ToString(),
                Items = x
                    .Items.OrderBy(i => i.Id)
                    .Select(i => new CartItemDto
                    {
                        Id = i.Id,
                        ProductVariantId = i.ProductVariantId,
                        ProductName = i.ProductVariant.Product.Name,
                        Quantity = i.Quantity,
                        UnitPrice =
                            i.ProductVariant.OriginalPrice.Amount
                            * (1 - i.ProductVariant.DiscountPercentage.Value / 100),
                        LineTotal =
                            i.Quantity
                            * i.ProductVariant.OriginalPrice.Amount
                            * (1 - i.ProductVariant.DiscountPercentage.Value / 100),
                    })
                    .ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
