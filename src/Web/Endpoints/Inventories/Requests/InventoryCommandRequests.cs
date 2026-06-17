namespace Fashia.Web.Endpoints.Inventories.Requests;

public sealed record InventoryStockRequest
{
    public int BranchId { get; init; }
    public int ProductVariantId { get; init; }
    public int Quantity { get; init; }
    public string? Note { get; init; }
}

public sealed record TransferInventoryStockRequest
{
    public int SourceBranchId { get; init; }
    public int DestinationBranchId { get; init; }
    public int ProductVariantId { get; init; }
    public int Quantity { get; init; }
    public string? Note { get; init; }
}

public sealed record InventoryReservationRequest
{
    public int BranchId { get; init; }
    public int ProductVariantId { get; init; }
    public int OrderId { get; init; }
    public int Quantity { get; init; }
    public string? Note { get; init; }
}

public sealed record ReturnInventoryStockRequest
{
    public int BranchId { get; init; }
    public int ProductVariantId { get; init; }
    public int? OrderId { get; init; }
    public int Quantity { get; init; }
    public string? Note { get; init; }
}
