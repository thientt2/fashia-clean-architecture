using Fashia.Application.Carts.Common;
using Fashia.Application.Carts.Queries.GetCurrentCart;
using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Entities;
using Fashia.Domain.Enums;

namespace Fashia.Application.Carts.Commands.AddCartItem;

public sealed record AddCartItemCommand : IRequest<CartDto>
{
    public int ProductVariantId { get; init; }
    public int Quantity { get; init; }
}

public sealed class AddCartItemCommandHandler : IRequestHandler<AddCartItemCommand, CartDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public AddCartItemCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<CartDto> Handle(
        AddCartItemCommand request,
        CancellationToken cancellationToken
    )
    {
        var productVariant = await _context
            .ProductVariants.Include(x => x.Product)
            .FirstOrDefaultAsync(x => x.Id == request.ProductVariantId, cancellationToken);

        if (productVariant is null)
        {
            throw new ValidationException("Product variant not found.");
        }

        if (productVariant.Product.Status != ProductStatus.Active)
        {
            throw new ValidationException("Product is not active.");
        }

        var customer = await _context
            .Customers.Where(x => x.UserId == _user.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (customer is null)
        {
            throw new UnauthorizedAccessException("Customer account is required.");
        }

        var cart = await _context
            .Carts.Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.CustomerId == customer.Id, cancellationToken);

        if (cart is null)
        {
            cart = Cart.Create(customer);
            _context.Carts.Add(cart);
        }

        var requestedQuantity =
            cart.Items.FirstOrDefault(x => x.ProductVariantId == request.ProductVariantId)?.Quantity
            ?? 0;
        requestedQuantity += request.Quantity;

        var inventoryQuery = _context.BranchVariantInventories.Where(x =>
            x.ProductVariantId == request.ProductVariantId
        );
        var hasInventory = await inventoryQuery.AnyAsync(cancellationToken);

        if (hasInventory)
        {
            var availableQuantity = await inventoryQuery.SumAsync(
                x => x.StockQuantity,
                cancellationToken
            );

            if (availableQuantity < requestedQuantity)
            {
                throw new ValidationException("Insufficient inventory.");
            }
        }

        cart.AddItem(
            request.ProductVariantId,
            request.Quantity,
            productVariant.SellingPrice.Amount
        );

        await _context.SaveChangesAsync(cancellationToken);

        return (await CartProjection.ProjectCartAsync(_context, cart.Id, cancellationToken))!;
    }
}
