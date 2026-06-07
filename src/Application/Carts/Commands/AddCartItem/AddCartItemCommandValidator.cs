namespace Fashia.Application.Carts.Commands.AddCartItem;

public sealed class AddCartItemCommandValidator : AbstractValidator<AddCartItemCommand>
{
    public AddCartItemCommandValidator()
    {
        RuleFor(x => x.ProductVariantId).GreaterThan(0);

        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
