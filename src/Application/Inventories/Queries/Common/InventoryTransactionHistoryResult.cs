namespace Fashia.Application.Inventories.Queries.Common;

public sealed record InventoryTransactionHistoryResult
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public IReadOnlyCollection<InventoryTransactionDto> Items { get; init; } = [];
}
