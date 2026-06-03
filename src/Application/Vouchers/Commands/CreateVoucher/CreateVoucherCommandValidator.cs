using Fashia.Domain.Enums;

namespace Fashia.Application.Vouchers.Commands.CreateVoucher;

public sealed class CreateVoucherCommandValidator : AbstractValidator<CreateVoucherCommand>
{
    public CreateVoucherCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.DiscountAmount).GreaterThan(0);
        RuleFor(x => x.DiscountAmount)
            .LessThanOrEqualTo(100)
            .When(x => x.DiscountType == DiscountType.Percentage);
        RuleFor(x => x.MinOrderAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxDiscountAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.UsageLimit).GreaterThanOrEqualTo(0);
        RuleFor(x => x.QuantityPerUser).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ValidUntil).GreaterThan(x => x.ValidFrom);
    }
}
