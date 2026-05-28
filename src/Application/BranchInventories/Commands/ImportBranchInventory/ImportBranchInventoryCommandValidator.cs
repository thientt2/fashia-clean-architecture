using FluentValidation;

namespace Fashia.Application.BranchInventories.Commands.ImportBranchInventory;

public class ImportBranchInventoryCommandValidator : AbstractValidator<ImportBranchInventoryCommand>
{
    public ImportBranchInventoryCommandValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.ProductVariantId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Note).MaximumLength(500);
    }
}
