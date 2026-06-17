namespace Fashia.Application.Inventories.Commands.DecreaseInventoryStock;

public sealed class DecreaseInventoryStockCommandValidator
    : AbstractValidator<DecreaseInventoryStockCommand>
{
    public DecreaseInventoryStockCommandValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.ProductVariantId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Note).MaximumLength(500);
    }
}
