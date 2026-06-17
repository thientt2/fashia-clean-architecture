using Fashia.Application.Common.Interfaces;
using Fashia.Application.Inventories.Queries.Common;

namespace Fashia.Application.Inventories.Queries.GetInventoryByProduct;

public sealed record GetInventoryByProductQuery(int ProductId) : IRequest<IReadOnlyCollection<InventoryDto>>;

public sealed class GetInventoryByProductQueryHandler
    : IRequestHandler<GetInventoryByProductQuery, IReadOnlyCollection<InventoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetInventoryByProductQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<InventoryDto>> Handle(
        GetInventoryByProductQuery request,
        CancellationToken cancellationToken
    )
    {
        var inventories = await _context
            .BranchVariantInventories.AsNoTracking()
            .Include(x => x.Branch)
            .Include(x => x.ProductVariant)
                .ThenInclude(x => x.Product)
            .Include(x => x.ProductVariant)
                .ThenInclude(x => x.AttributeValues)
                    .ThenInclude(x => x.AttributeValue)
            .Where(x => x.ProductVariant.ProductId == request.ProductId)
            .OrderBy(x => x.BranchId)
            .ThenBy(x => x.ProductVariantId)
            .ToListAsync(cancellationToken);

        return inventories.Select(x => x.ToDto()).ToList();
    }
}
