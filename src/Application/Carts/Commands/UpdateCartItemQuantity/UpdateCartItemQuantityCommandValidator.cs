namespace Fashia.Application.Carts.Commands.UpdateCartItemQuantity;

public sealed class UpdateCartItemQuantityCommandValidator
    : AbstractValidator<UpdateCartItemQuantityCommand>
{
    public UpdateCartItemQuantityCommandValidator()
    {
        RuleFor(x => x.CartItemId).GreaterThan(0);

        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
    }
}
