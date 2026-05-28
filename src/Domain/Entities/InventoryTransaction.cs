namespace Fashia.Domain.Entities;

public class InventoryTransaction : BaseAuditableEntity
{
    private InventoryTransaction() { }

    public InventoryTransaction(
        int branchId,
        int productVariantId,
        InventoryTransactionType type,
        int quantity,
        string? note = null
    )
    {
        SetBranchId(branchId);
        SetProductVariantId(productVariantId);
        SetType(type);
        SetQuantity(quantity);
        SetNote(note);
    }

    public int BranchId { get; private set; }
    public Branch Branch { get; private set; } = null!;

    public int ProductVariantId { get; private set; }
    public ProductVariant ProductVariant { get; private set; } = null!;

    public InventoryTransactionType Type { get; private set; }

    public int Quantity { get; private set; }

    public string? Note { get; private set; }

    private void SetBranchId(int branchId)
    {
        if (branchId <= 0)
            throw new ArgumentException("Branch ID must be greater than zero.", nameof(branchId));

        BranchId = branchId;
    }

    private void SetProductVariantId(int productVariantId)
    {
        if (productVariantId <= 0)
            throw new ArgumentException(
                "Product Variant ID must be greater than zero.",
                nameof(productVariantId)
            );

        ProductVariantId = productVariantId;
    }

    private void SetType(InventoryTransactionType type)
    {
        if (!Enum.IsDefined(typeof(InventoryTransactionType), type))
            throw new ArgumentException("Invalid inventory transaction type.", nameof(type));

        Type = type;
    }

    private void SetNote(string? note)
    {
        if (note != null && note.Length > 500)
            throw new ArgumentException("Note cannot exceed 500 characters.", nameof(note));

        Note = note;
    }

    private void SetQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        Quantity = quantity;
    }
}
