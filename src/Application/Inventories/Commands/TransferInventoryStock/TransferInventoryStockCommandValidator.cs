namespace Fashia.Application.Inventories.Commands.TransferInventoryStock;

public sealed class TransferInventoryStockCommandValidator
    : AbstractValidator<TransferInventoryStockCommand>
{
    public TransferInventoryStockCommandValidator()
    {
        RuleFor(x => x.SourceBranchId).GreaterThan(0);
        RuleFor(x => x.DestinationBranchId).GreaterThan(0);
        RuleFor(x => x.ProductVariantId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Note).MaximumLength(500);
    }
}
