namespace Fashia.Application.Inventories.Commands.ReturnInventoryStock;

public sealed class ReturnInventoryStockCommandValidator
    : AbstractValidator<ReturnInventoryStockCommand>
{
    public ReturnInventoryStockCommandValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.ProductVariantId).GreaterThan(0);
        RuleFor(x => x.OrderId).GreaterThan(0).When(x => x.OrderId.HasValue);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Note).MaximumLength(500);
    }
}
