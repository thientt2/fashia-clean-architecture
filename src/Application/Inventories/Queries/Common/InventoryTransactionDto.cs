namespace Fashia.Application.Inventories.Queries.Common;

public sealed record InventoryTransactionDto
{
    public int Id { get; init; }
    public int BranchId { get; init; }
    public string BranchName { get; init; } = string.Empty;
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int ProductVariantId { get; init; }
    public string Type { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public int? PreviousStockQuantity { get; init; }
    public int? NewStockQuantity { get; init; }
    public int? PreviousReservedQuantity { get; init; }
    public int? NewReservedQuantity { get; init; }
    public int? SourceBranchId { get; init; }
    public int? DestinationBranchId { get; init; }
    public int? OrderId { get; init; }
    public Guid? TransferCorrelationId { get; init; }
    public string? Note { get; init; }
    public DateTimeOffset Created { get; init; }
}
