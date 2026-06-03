using Fashia.Application.Carts.Common;
using Fashia.Application.Carts.Queries;
using Fashia.Application.Common.Interfaces;

namespace Fashia.Application.Carts.Commands.CreateCart;

public sealed record CreateCartCommand : IRequest<CartDto>;

public sealed class CreateCartCommandHandler : IRequestHandler<CreateCartCommand, CartDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CreateCartCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<CartDto> Handle(
        CreateCartCommand request,
        CancellationToken cancellationToken
    )
    {
        var customerId = await CartHelpers.ResolveCustomerIdAsync(_context, _user, cancellationToken);
        var cart = await CartHelpers.FindCartAsync(_context, customerId, true, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return (await CartHelpers.ProjectCartAsync(_context, cart!.Id, cancellationToken))!;
    }
}
