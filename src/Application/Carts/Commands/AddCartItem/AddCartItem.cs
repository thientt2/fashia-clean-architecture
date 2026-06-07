using Fashia.Application.Carts.Common;
using Fashia.Application.Carts.Queries.GetCurrentCart;
using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Entities;

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
        var productVariantExists = await _context.ProductVariants.AnyAsync(
            x => x.Id == request.ProductVariantId,
            cancellationToken
        );

        if (!productVariantExists)
        {
            throw new InvalidOperationException("Product variant not found.");
        }

        var customerId = await _context
            .Customers.Where(x => x.UserId == _user.Id)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (customerId == 0)
        {
            throw new UnauthorizedAccessException("Customer account is required.");
        }

        var cart = await _context
            .Carts.Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.CustomerId == customerId, cancellationToken);

        if (cart is null)
        {
            cart = new Cart(customerId);
            _context.Carts.Add(cart);
        }

        cart.AddItem(request.ProductVariantId, request.Quantity);

        await _context.SaveChangesAsync(cancellationToken);

        return (await CartProjection.ProjectCartAsync(_context, cart.Id, cancellationToken))!;
    }
}
