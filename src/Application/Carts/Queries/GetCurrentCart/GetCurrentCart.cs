using Fashia.Application.Carts.Common;
using Fashia.Application.Common.Interfaces;

namespace Fashia.Application.Carts.Queries.GetCurrentCart;

public sealed record GetCurrentCartQuery : IRequest<CartDto>;

public sealed class GetCurrentCartQueryHandler : IRequestHandler<GetCurrentCartQuery, CartDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetCurrentCartQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<CartDto> Handle(
        GetCurrentCartQuery request,
        CancellationToken cancellationToken
    )
    {
        var customerId = await _context
            .Customers.Where(x => x.UserId == _user.Id)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (customerId == 0)
        {
            return CartDto.Empty();
        }

        var cart = await _context
            .Carts.AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .ProjectToDto()
            .FirstOrDefaultAsync(cancellationToken);

        return cart ?? CartDto.Empty();
    }
}
