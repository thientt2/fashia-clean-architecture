namespace Fashia.Application.Orders.Commands.CheckoutCart;

public sealed class CheckoutCartCommandValidator : AbstractValidator<CheckoutCartCommand>
{
    public CheckoutCartCommandValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.VoucherCode).MaximumLength(50);
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CustomerEmail).MaximumLength(256);
        RuleFor(x => x.CustomerPhone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.ShippingAddress).NotEmpty().MaximumLength(500);
    }
}
