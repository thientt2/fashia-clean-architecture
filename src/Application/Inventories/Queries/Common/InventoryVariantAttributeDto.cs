namespace Fashia.Application.Inventories.Queries.Common;

public sealed record InventoryVariantAttributeDto
{
    public int Id { get; init; }
    public string Value { get; init; } = string.Empty;
    public string? HexValue { get; init; }
}
