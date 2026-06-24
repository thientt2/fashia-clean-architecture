using Fashia.Application.Common.Interfaces;

namespace Fashia.Application.Orders.Commands.PlaceOrder;

public sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public PlaceOrderCommandValidator(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;

        RuleFor(x => x.IdempotencyKey)
            .Must(IsUuidV4)
            .WithMessage("Idempotency key must be a UUID v4.");

        RuleFor(x => x.ShippingAddress)
            .NotNull()
            .WithMessage("Shipping address is required.")
            .SetValidator(new PlaceOrderShippingAddressDtoValidator()!);

        RuleFor(x => x.PaymentMethod).IsInEnum();

        RuleFor(x => x.VoucherCode).MaximumLength(50);

        RuleFor(x => x.Note).MaximumLength(1000);

        RuleFor(x => x)
            .MustAsync(HaveNonEmptyAuthenticatedCartAsync)
            .WithMessage("Authenticated customer cart is required and must not be empty.")
            .OverridePropertyName("Cart");
    }

    private async Task<bool> HaveNonEmptyAuthenticatedCartAsync(
        PlaceOrderCommand command,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(_user.Id))
            return false;

        var customerId = await _context
            .Customers.Where(x => x.UserId == _user.Id)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (customerId == 0)
            return false;

        return await _context.Carts.AnyAsync(
            x => x.CustomerId == customerId && x.Items.Any(),
            cancellationToken
        );
    }

    private static bool IsUuidV4(string value)
    {
        return Guid.TryParse(value, out var key) && key.ToString("D")[14] == '4';
    }
}

public sealed class PlaceOrderShippingAddressDtoValidator
    : AbstractValidator<PlaceOrderShippingAddressDto>
{
    public PlaceOrderShippingAddressDtoValidator()
    {
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.CustomerPhone).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Line1).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Ward).NotEmpty().MaximumLength(100);
        RuleFor(x => x.District).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Province).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Latitude).InclusiveBetween(-90M, 90M);
        RuleFor(x => x.Longitude).InclusiveBetween(-180M, 180M);
    }
}
