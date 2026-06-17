namespace Fashia.Domain.Entities;

public class InventoryTransaction : BaseAuditableEntity
{
    private InventoryTransaction() { }

    public InventoryTransaction(
        int branchId,
        int productVariantId,
        InventoryTransactionType type,
        int quantity,
        string? note = null,
        int? previousStockQuantity = null,
        int? newStockQuantity = null,
        int? previousReservedQuantity = null,
        int? newReservedQuantity = null,
        int? sourceBranchId = null,
        int? destinationBranchId = null,
        int? orderId = null,
        Guid? transferCorrelationId = null
    )
    {
        SetBranchId(branchId);
        SetProductVariantId(productVariantId);
        SetType(type);
        SetQuantity(quantity);
        SetNote(note);
        SetStockSnapshot(
            previousStockQuantity,
            newStockQuantity,
            previousReservedQuantity,
            newReservedQuantity
        );
        SetBranchMetadata(sourceBranchId, destinationBranchId);
        SetOrderId(orderId);
        TransferCorrelationId = transferCorrelationId;
    }

    public int BranchId { get; private set; }
    public Branch Branch { get; private set; } = null!;

    public int ProductVariantId { get; private set; }
    public ProductVariant ProductVariant { get; private set; } = null!;

    public InventoryTransactionType Type { get; private set; }

    public int Quantity { get; private set; }

    public string? Note { get; private set; }

    public int? PreviousStockQuantity { get; private set; }

    public int? NewStockQuantity { get; private set; }

    public int? PreviousReservedQuantity { get; private set; }

    public int? NewReservedQuantity { get; private set; }

    public int? SourceBranchId { get; private set; }

    public int? DestinationBranchId { get; private set; }

    public int? OrderId { get; private set; }

    public Guid? TransferCorrelationId { get; private set; }

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

    private void SetStockSnapshot(
        int? previousStockQuantity,
        int? newStockQuantity,
        int? previousReservedQuantity,
        int? newReservedQuantity
    )
    {
        EnsureNonNegative(previousStockQuantity, nameof(previousStockQuantity));
        EnsureNonNegative(newStockQuantity, nameof(newStockQuantity));
        EnsureNonNegative(previousReservedQuantity, nameof(previousReservedQuantity));
        EnsureNonNegative(newReservedQuantity, nameof(newReservedQuantity));

        PreviousStockQuantity = previousStockQuantity;
        NewStockQuantity = newStockQuantity;
        PreviousReservedQuantity = previousReservedQuantity;
        NewReservedQuantity = newReservedQuantity;
    }

    private void SetBranchMetadata(int? sourceBranchId, int? destinationBranchId)
    {
        EnsurePositive(sourceBranchId, nameof(sourceBranchId));
        EnsurePositive(destinationBranchId, nameof(destinationBranchId));

        SourceBranchId = sourceBranchId;
        DestinationBranchId = destinationBranchId;
    }

    private void SetOrderId(int? orderId)
    {
        EnsurePositive(orderId, nameof(orderId));
        OrderId = orderId;
    }

    private static void EnsureNonNegative(int? value, string paramName)
    {
        if (value < 0)
            throw new ArgumentException("Value cannot be negative.", paramName);
    }

    private static void EnsurePositive(int? value, string paramName)
    {
        if (value <= 0)
            throw new ArgumentException("Value must be greater than zero.", paramName);
    }
}
