using Fashia.Application.Carts.Common;
using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Fashia.Application.Carts.Queries.GetCurrentCart;

public sealed record GetCurrentCartQuery : IRequest<CartDto>;

public sealed class GetCurrentCartQueryHandler : IRequestHandler<GetCurrentCartQuery, CartDto>
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
            throw new UnauthorizedAccessException("Customer account is required.");
        }

        var cart = await _context
            .Carts.AsNoTracking()
            .Where(x => x.CustomerId == customerId)
            .ProjectToDto()
            .FirstOrDefaultAsync(cancellationToken);

        return cart ?? CartDto.Empty();
    }
}
