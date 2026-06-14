using Fashia.Application.Carts.Common;
using Fashia.Application.Carts.Queries.GetCurrentCart;
using Fashia.Application.Common.Interfaces;

namespace Fashia.Application.Carts.Commands.RemoveCartItem;

public sealed record RemoveCartItemCommand : IRequest<CartDto>
{
    public int CartItemId { get; init; }
}

public sealed class RemoveCartItemCommandHandler : IRequestHandler<RemoveCartItemCommand, CartDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public RemoveCartItemCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<CartDto> Handle(
        RemoveCartItemCommand request,
        CancellationToken cancellationToken
    )
    {
        var customerId = await CartLookup.GetCurrentCustomerIdAsync(
            _context,
            _user,
            cancellationToken
        );

        var cart = await CartLookup.GetActiveCartWithItemsAsync(
            _context,
            customerId,
            cancellationToken
        );

        if (cart is null)
        {
            throw new InvalidOperationException("Cart not found.");
        }

        cart.RemoveItem(request.CartItemId);

        await _context.SaveChangesAsync(cancellationToken);

        return (await CartProjection.ProjectCartAsync(_context, cart.Id, cancellationToken))!;
    }
}
