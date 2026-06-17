using Fashia.Application.Common.Interfaces;
using Fashia.Application.Inventories.Queries.Common;

namespace Fashia.Application.Inventories.Queries.GetInventoryByBranch;

public sealed record GetInventoryByBranchQuery(int BranchId) : IRequest<IReadOnlyCollection<InventoryDto>>;

public sealed class GetInventoryByBranchQueryHandler
    : IRequestHandler<GetInventoryByBranchQuery, IReadOnlyCollection<InventoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetInventoryByBranchQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<InventoryDto>> Handle(
        GetInventoryByBranchQuery request,
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
            .Where(x => x.BranchId == request.BranchId)
            .OrderBy(x => x.ProductVariantId)
            .ToListAsync(cancellationToken);

        return inventories.Select(x => x.ToDto()).ToList();
    }
}
