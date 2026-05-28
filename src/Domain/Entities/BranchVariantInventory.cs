namespace Fashia.Domain.Entities;

public class BranchVariantInventory : BaseAuditableEntity
{
    private BranchVariantInventory()
    {
        // EF Core
    }

    public BranchVariantInventory(int branchId, int variantId)
    {
        SetBranchId(branchId);
        SetProductVariantId(variantId);
    }

    public int BranchId { get; private set; }
    public Branch Branch { get; private set; } = null!;

    public int ProductVariantId { get; private set; }
    public ProductVariant ProductVariant { get; private set; } = null!;

    public int StockQuantity { get; private set; }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException(
                "Quantity to increase must be greater than zero.",
                nameof(quantity)
            );

        StockQuantity += quantity;
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException(
                "Quantity to decrease must be greater than zero.",
                nameof(quantity)
            );

        if (quantity > StockQuantity)
            throw new InvalidOperationException("Cannot decrease stock below zero.");

        StockQuantity -= quantity;
    }

    public void AdjustStock(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative.", nameof(quantity));

        StockQuantity = quantity;
    }

    private void SetBranchId(int branchId)
    {
        if (branchId <= 0)
            throw new ArgumentException("Branch ID must be greater than zero.", nameof(branchId));

        BranchId = branchId;
    }

    private void SetProductVariantId(int variantId)
    {
        if (variantId <= 0)
            throw new ArgumentException(
                "Product variant ID must be greater than zero.",
                nameof(variantId)
            );

        ProductVariantId = variantId;
    }
}
