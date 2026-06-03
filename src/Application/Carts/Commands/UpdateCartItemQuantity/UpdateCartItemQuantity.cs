using Fashia.Application.Carts.Common;
using Fashia.Application.Carts.Queries;
using Fashia.Application.Common.Interfaces;

namespace Fashia.Application.Carts.Commands.UpdateCartItemQuantity;

public sealed record UpdateCartItemQuantityCommand : IRequest<CartDto>
{
    public int ProductVariantId { get; init; }
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
        var customerId = await CartHelpers.ResolveCustomerIdAsync(_context, _user, cancellationToken);
        var cart =
            await CartHelpers.FindCartAsync(_context, customerId, false, cancellationToken)
            ?? throw new InvalidOperationException("Cart not found.");

        cart.UpdateItemQuantity(request.ProductVariantId, request.Quantity);

        await _context.SaveChangesAsync(cancellationToken);

        return (await CartHelpers.ProjectCartAsync(_context, cart.Id, cancellationToken))!;
    }
}
