namespace Fashia.Application.Inventories.Commands.ReserveInventoryStock;

public sealed class ReserveInventoryStockCommandValidator
    : AbstractValidator<ReserveInventoryStockCommand>
{
    public ReserveInventoryStockCommandValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.ProductVariantId).GreaterThan(0);
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Note).MaximumLength(500);
    }
}
