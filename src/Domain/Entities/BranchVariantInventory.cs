namespace Fashia.Domain.Entities;

public class BranchVariantInventory : BaseAuditableEntity
{
    private BranchVariantInventory()
    {
        // EF Core
    }

    private BranchVariantInventory(int branchId, int variantId, int initialQuantity = 0)
    {
        SetBranchId(branchId);
        SetProductVariantId(variantId);
        StockQuantity = initialQuantity;
    }

    public static BranchVariantInventory Create(
        int branchId,
        int variantId,
        int initialQuantity = 0
    )
    {
        return new BranchVariantInventory(branchId, variantId, initialQuantity);
    }

    public int BranchId { get; private set; }
    public Branch Branch { get; private set; } = null!;

    public int ProductVariantId { get; private set; }
    public ProductVariant ProductVariant { get; private set; } = null!;

    public int StockQuantity { get; private set; }
    public int ReservedQuantity { get; private set; }
    public int AvailableQuantity => StockQuantity - ReservedQuantity;

    public void IncreaseStock(int quantity)
    {
        EnsurePositiveQuantity(quantity, nameof(quantity));
        StockQuantity += quantity;
    }

    public void DecreaseStock(int quantity)
    {
        EnsurePositiveQuantity(quantity, nameof(quantity));

        if (quantity > AvailableQuantity)
            throw new InvalidOperationException("Cannot decrease stock below available quantity.");

        StockQuantity -= quantity;
    }

    public void AdjustStock(int quantity)
    {
        EnsureNonNegativeQuantity(quantity, nameof(quantity));

        if (quantity < ReservedQuantity)
        {
            throw new InvalidOperationException("Cannot adjust stock below reserved quantity.");
        }

        StockQuantity = quantity;
    }

    public void ReserveStock(int quantity)
    {
        EnsurePositiveQuantity(quantity, nameof(quantity));

        if (quantity > AvailableQuantity)
            throw new InvalidOperationException("Cannot reserve more than available quantity.");

        ReservedQuantity += quantity;
    }

    public void ReleaseReservedStock(int quantity)
    {
        EnsurePositiveQuantity(quantity, nameof(quantity));

        if (quantity > ReservedQuantity)
            throw new InvalidOperationException("Cannot release more than reserved quantity.");

        ReservedQuantity -= quantity;
    }

    public void CommitReservedStock(int quantity)
    {
        EnsurePositiveQuantity(quantity, nameof(quantity));

        if (quantity > ReservedQuantity)
            throw new InvalidOperationException("Cannot commit more than reserved quantity.");

        ReservedQuantity -= quantity;
        StockQuantity -= quantity;
    }

    public void ReturnStock(int quantity)
    {
        IncreaseStock(quantity);
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

    private static void EnsurePositiveQuantity(int quantity, string paramName)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", paramName);
    }

    private static void EnsureNonNegativeQuantity(int quantity, string paramName)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative.", paramName);
    }
}
