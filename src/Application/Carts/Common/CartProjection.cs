using Fashia.Application.Carts.Queries.GetCurrentCart;
using Fashia.Application.Common.Interfaces;

namespace Fashia.Application.Carts.Common;

internal static class CartProjection
{
    internal static IQueryable<CartDto> ProjectToDto(this IQueryable<Domain.Entities.Cart> carts)
    {
        return carts.Select(x => new CartDto
        {
            Id = x.Id,
            Items = x
                .Items.OrderBy(item => item.Id)
                .Select(item => new CartItemDto
                {
                    Id = item.Id,
                    ProductVariantId = item.ProductVariantId,
                    ProductName = item.ProductVariant.Product.Name,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    LineTotal = item.Quantity * item.UnitPrice,
                })
                .ToList(),
        });
    }

    internal static Task<CartDto?> ProjectCartAsync(
        IApplicationDbContext context,
        int cartId,
        CancellationToken cancellationToken
    )
    {
        return context
            .Carts.AsNoTracking()
            .Where(x => x.Id == cartId)
            .ProjectToDto()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
