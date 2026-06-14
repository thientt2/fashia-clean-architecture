using Fashia.Application.Carts.Common;
using Fashia.Application.Common.Interfaces;

namespace Fashia.Application.Carts.Commands.ClearCart;

public sealed record ClearCartCommand : IRequest;

public sealed class ClearCartCommandHandler : IRequestHandler<ClearCartCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public ClearCartCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(ClearCartCommand request, CancellationToken cancellationToken)
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
            return;
        }

        cart.Clear();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
