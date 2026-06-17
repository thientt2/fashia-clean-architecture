namespace Fashia.Application.Inventories.Commands.IncreaseInventoryStock;

public sealed class IncreaseInventoryStockCommandValidator
    : AbstractValidator<IncreaseInventoryStockCommand>
{
    public IncreaseInventoryStockCommandValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.ProductVariantId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Note).MaximumLength(500);
    }
}
