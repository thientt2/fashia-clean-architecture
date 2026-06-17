using Fashia.Domain.Entities;

namespace Fashia.Application.Inventories.Queries.Common;

internal static class InventoryProjection
{
    internal static InventoryDto ToDto(this BranchVariantInventory inventory)
    {
        return new InventoryDto
        {
            BranchId = inventory.BranchId,
            BranchName = inventory.Branch.Name,
            ProductId = inventory.ProductVariant.ProductId,
            ProductName = inventory.ProductVariant.Product.Name,
            ProductVariantId = inventory.ProductVariantId,
            SellingPrice = inventory.ProductVariant.SellingPrice.Amount,
            StockQuantity = inventory.StockQuantity,
            ReservedQuantity = inventory.ReservedQuantity,
            AvailableQuantity = inventory.AvailableQuantity,
            AttributeValues = inventory
                .ProductVariant.AttributeValues.OrderBy(x => x.AttributeValue.Value)
                .Select(x => new InventoryVariantAttributeDto
                {
                    Id = x.AttributeValueId,
                    Value = x.AttributeValue.Value,
                    HexValue = x.AttributeValue.HexValue,
                })
                .ToList(),
        };
    }
}
