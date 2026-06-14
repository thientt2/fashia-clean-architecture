namespace Fashia.Application.Orders.Commands.CheckoutOrder;

public sealed class CheckoutOrderCommandValidator : AbstractValidator<CheckoutOrderCommand>
{
    public CheckoutOrderCommandValidator()
    {
        RuleFor(x => x.CartItemIds).NotEmpty();
        RuleForEach(x => x.CartItemIds).GreaterThan(0);
        RuleFor(x => x.ShippingAddressId).GreaterThan(0);
        RuleFor(x => x.PaymentMethod).IsInEnum();
        RuleFor(x => x.Note).MaximumLength(1000);
    }
}
