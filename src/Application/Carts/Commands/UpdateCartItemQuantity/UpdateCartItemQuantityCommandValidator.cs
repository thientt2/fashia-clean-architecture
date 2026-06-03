namespace Fashia.Application.Carts.Commands.UpdateCartItemQuantity;

public sealed class UpdateCartItemQuantityCommandValidator
    : AbstractValidator<UpdateCartItemQuantityCommand>
{
    public UpdateCartItemQuantityCommandValidator()
    {
        RuleFor(x => x.ProductVariantId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
