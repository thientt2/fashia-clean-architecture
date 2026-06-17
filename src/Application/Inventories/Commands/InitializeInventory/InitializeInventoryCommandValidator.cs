namespace Fashia.Application.Inventories.Commands.InitializeInventory;

public sealed class InitializeInventoryCommandValidator
    : AbstractValidator<InitializeInventoryCommand>
{
    public InitializeInventoryCommandValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.ProductVariantId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Note).MaximumLength(500);
    }
}
