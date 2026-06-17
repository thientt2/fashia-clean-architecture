namespace Fashia.Application.Inventories.Commands.AdjustInventoryStock;

public sealed class AdjustInventoryStockCommandValidator
    : AbstractValidator<AdjustInventoryStockCommand>
{
    public AdjustInventoryStockCommandValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.ProductVariantId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Note).MaximumLength(500);
    }
}
