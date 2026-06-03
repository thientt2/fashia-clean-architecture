using Fashia.Application.Carts.Common;
using Fashia.Application.Carts.Queries;
using Fashia.Application.Common.Interfaces;

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
        await CartHelpers.ValidateProductVariantAsync(
            _context,
            request.ProductVariantId,
            cancellationToken
        );

        var customerId = await CartHelpers.ResolveCustomerIdAsync(_context, _user, cancellationToken);
        var cart = await CartHelpers.FindCartAsync(_context, customerId, true, cancellationToken);

        cart!.AddItem(request.ProductVariantId, request.Quantity);

        await _context.SaveChangesAsync(cancellationToken);

        return (await CartHelpers.ProjectCartAsync(_context, cart.Id, cancellationToken))!;
    }
}
