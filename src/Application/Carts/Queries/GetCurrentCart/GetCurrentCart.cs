using Fashia.Application.Carts.Common;
using Fashia.Application.Common.Interfaces;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;

namespace Fashia.Application.Carts.Queries.GetCurrentCart;

public sealed record GetCurrentCartQuery : IRequest<CartDto?>;

public sealed class GetCurrentCartQueryHandler : IRequestHandler<GetCurrentCartQuery, CartDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ILogger<GetCurrentCartQueryHandler> _logger;

    public GetCurrentCartQueryHandler(
        IApplicationDbContext context,
        IUser user,
        ILogger<GetCurrentCartQueryHandler> logger
    )
    {
        _context = context;
        _user = user;
        _logger = logger;
    }

    public async Task<CartDto?> Handle(
        GetCurrentCartQuery request,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation("Handling GetCurrentCartQuery for user: {UserId}", _user.Id);
        var customerId = await CartHelpers.ResolveCustomerIdAsync(
            _context,
            _user,
            cancellationToken
        );
        var cart = await CartHelpers.FindCartAsync(_context, customerId, false, cancellationToken);

        _logger.LogInformation("Retrieved cart for customer: {CustomerId}", customerId);

        return cart is null
            ? null
            : await CartHelpers.ProjectCartAsync(_context, cart.Id, cancellationToken);
    }
}
