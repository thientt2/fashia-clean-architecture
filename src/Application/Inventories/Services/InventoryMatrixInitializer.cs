using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Entities;
using Fashia.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Fashia.Application.Inventories.Services;

public sealed class InventoryMatrixInitializer : IInventoryMatrixInitializer
{
    private readonly IApplicationDbContext _context;

    public InventoryMatrixInitializer(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task EnsureForProductVariantsAsync(
        IReadOnlyCollection<int> productVariantIds,
        CancellationToken cancellationToken
    )
    {
        var variantIds = productVariantIds.Where(x => x > 0).Distinct().ToArray();

        if (variantIds.Length == 0)
            throw new ArgumentException(
                "Product variant ids must contain at least one valid id.",
                nameof(productVariantIds)
            );

        var activeBranchIds = await _context
            .Branches.Where(x => x.Status == BranchStatus.Active)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (activeBranchIds.Count == 0)
            return;

        var existingKeys = await _context
            .BranchVariantInventories.Where(x =>
                variantIds.Contains(x.ProductVariantId) && activeBranchIds.Contains(x.BranchId)
            )
            .Select(x => new { x.BranchId, x.ProductVariantId })
            .ToListAsync(cancellationToken);

        var existingSet = existingKeys.Select(x => (x.BranchId, x.ProductVariantId)).ToHashSet();

        foreach (var variantId in variantIds)
        {
            foreach (var branchId in activeBranchIds)
            {
                if (existingSet.Contains((branchId, variantId)))
                    continue;

                var inventory = BranchVariantInventory.Create(
                    branchId,
                    variantId,
                    initialQuantity: 0
                );

                _context.BranchVariantInventories.Add(inventory);
            }
        }
    }

    public async Task EnsureForBranchAsync(int branchId, CancellationToken cancellationToken)
    {
        var productVariantIds = await _context
            .ProductVariants.Where(x => x.Status == ProductVariantStatus.Active)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        await EnsureForProductVariantsAsync(productVariantIds, cancellationToken);
    }
}
