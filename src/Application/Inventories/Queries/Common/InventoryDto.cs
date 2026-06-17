namespace Fashia.Application.Inventories.Queries.Common;

public sealed record InventoryDto
{
    public int BranchId { get; init; }
    public string BranchName { get; init; } = string.Empty;
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int ProductVariantId { get; init; }
    public decimal SellingPrice { get; init; }
    public int StockQuantity { get; init; }
    public int ReservedQuantity { get; init; }
    public int AvailableQuantity { get; init; }
    public IReadOnlyCollection<InventoryVariantAttributeDto> AttributeValues { get; init; } = [];
}
