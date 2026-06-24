using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Enums;

namespace Fashia.Application.Vouchers.Commands.UpdateVoucher;

public sealed class UpdateVoucherCommandValidator : AbstractValidator<UpdateVoucherCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateVoucherCommandValidator(IApplicationDbContext context)
    {
        _context = context;

        RuleFor(x => x.Id).GreaterThan(0);
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

        RuleFor(x => x)
            .Must(HaveMatchingTarget)
            .WithMessage("Voucher target must match voucher type.")
            .OverridePropertyName("VoucherType");

        RuleFor(x => x.ProductId)
            .MustAsync(ProductExistsAsync)
            .WithMessage("Product not found.")
            .When(x => x.VoucherType == VoucherType.ProductSpecific);

        RuleFor(x => x.CategoryId)
            .MustAsync(CategoryExistsAsync)
            .WithMessage("Category not found.")
            .When(x => x.VoucherType == VoucherType.CategorySpecific);

        RuleFor(x => x.BrandId)
            .MustAsync(BrandExistsAsync)
            .WithMessage("Brand not found.")
            .When(x => x.VoucherType == VoucherType.BrandSpecific);
    }

    private static bool HaveMatchingTarget(UpdateVoucherCommand command) =>
        command.VoucherType switch
        {
            VoucherType.All =>
                command.ProductId is null
                && command.CategoryId is null
                && command.BrandId is null,
            VoucherType.ProductSpecific =>
                command.ProductId > 0
                && command.CategoryId is null
                && command.BrandId is null,
            VoucherType.CategorySpecific =>
                command.ProductId is null
                && command.CategoryId > 0
                && command.BrandId is null,
            VoucherType.BrandSpecific =>
                command.ProductId is null
                && command.CategoryId is null
                && command.BrandId > 0,
            _ => false,
        };

    private Task<bool> ProductExistsAsync(int? id, CancellationToken cancellationToken) =>
        id.HasValue
            ? _context.Products.AnyAsync(x => x.Id == id.Value, cancellationToken)
            : Task.FromResult(false);

    private Task<bool> CategoryExistsAsync(int? id, CancellationToken cancellationToken) =>
        id.HasValue
            ? _context.Categories.AnyAsync(x => x.Id == id.Value, cancellationToken)
            : Task.FromResult(false);

    private Task<bool> BrandExistsAsync(int? id, CancellationToken cancellationToken) =>
        id.HasValue
            ? _context.Brands.AnyAsync(x => x.Id == id.Value, cancellationToken)
            : Task.FromResult(false);
}
