using Fashia.Application.Common.Interfaces;
using Fashia.Application.Inventories.Queries.Common;

namespace Fashia.Application.Inventories.Queries.GetLowStockInventory;

public sealed record GetLowStockInventoryQuery : IRequest<IReadOnlyCollection<InventoryDto>>;

public sealed class GetLowStockInventoryQueryHandler
    : IRequestHandler<GetLowStockInventoryQuery, IReadOnlyCollection<InventoryDto>>
{
    private const int LowStockThreshold = 10;

    private readonly IApplicationDbContext _context;

    public GetLowStockInventoryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<InventoryDto>> Handle(
        GetLowStockInventoryQuery request,
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
            .Where(x => x.StockQuantity - x.ReservedQuantity <= LowStockThreshold)
            .OrderBy(x => x.BranchId)
            .ThenBy(x => x.ProductVariantId)
            .ToListAsync(cancellationToken);

        return inventories.Select(x => x.ToDto()).ToList();
    }
}
