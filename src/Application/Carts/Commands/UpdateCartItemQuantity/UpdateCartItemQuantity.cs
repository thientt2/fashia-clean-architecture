using Fashia.Application.Carts.Common;
using Fashia.Application.Carts.Queries.GetCurrentCart;
using Fashia.Application.Common.Interfaces;

namespace Fashia.Application.Carts.Commands.UpdateCartItemQuantity;

public sealed record UpdateCartItemQuantityCommand : IRequest<CartDto>
{
    public int CartItemId { get; init; }
    public int Quantity { get; init; }
}

public sealed class UpdateCartItemQuantityCommandHandler
    : IRequestHandler<UpdateCartItemQuantityCommand, CartDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UpdateCartItemQuantityCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<CartDto> Handle(
        UpdateCartItemQuantityCommand request,
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

        cart.UpdateItemQuantity(request.CartItemId, request.Quantity);

        await _context.SaveChangesAsync(cancellationToken);

        return (await CartProjection.ProjectCartAsync(_context, cart.Id, cancellationToken))!;
    }
}
