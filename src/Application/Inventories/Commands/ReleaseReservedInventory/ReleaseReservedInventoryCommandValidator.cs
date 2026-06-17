namespace Fashia.Application.Inventories.Commands.ReleaseReservedInventory;

public sealed class ReleaseReservedInventoryCommandValidator
    : AbstractValidator<ReleaseReservedInventoryCommand>
{
    public ReleaseReservedInventoryCommandValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.ProductVariantId).GreaterThan(0);
        RuleFor(x => x.OrderId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Note).MaximumLength(500);
    }
}
