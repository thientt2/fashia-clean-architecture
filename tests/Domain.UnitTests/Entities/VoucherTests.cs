using Fashia.Domain.Entities;
using Fashia.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace Fashia.Domain.UnitTests.Entities;

public class VoucherTests
{
    [Test]
    public void ShouldApplyProductVoucherOnlyToTargetProduct()
    {
        var voucher = CreateVoucher(VoucherType.ProductSpecific, productId: 10);

        voucher.IsApplicableTo(productIds: [10], categoryIds: [20], brandIds: [30]).ShouldBeTrue();
        voucher.IsApplicableTo(productIds: [11], categoryIds: [20], brandIds: [30]).ShouldBeFalse();
    }

    [Test]
    public void ShouldApplyCategoryVoucherOnlyToTargetCategory()
    {
        var voucher = CreateVoucher(VoucherType.CategorySpecific, categoryId: 20);

        voucher.IsApplicableTo(productIds: [10], categoryIds: [20], brandIds: [30]).ShouldBeTrue();
        voucher.IsApplicableTo(productIds: [10], categoryIds: [21], brandIds: [30]).ShouldBeFalse();
    }

    [Test]
    public void ShouldApplyBrandVoucherOnlyToTargetBrand()
    {
        var voucher = CreateVoucher(VoucherType.BrandSpecific, brandId: 30);

        voucher.IsApplicableTo(productIds: [10], categoryIds: [20], brandIds: [30]).ShouldBeTrue();
        voucher.IsApplicableTo(productIds: [10], categoryIds: [20], brandIds: [31]).ShouldBeFalse();
    }

    [Test]
    public void ShouldRejectTargetedVoucherWithoutMatchingTarget()
    {
        Should.Throw<ArgumentException>(() => CreateVoucher(VoucherType.ProductSpecific));
    }

    private static Voucher CreateVoucher(
        VoucherType voucherType,
        int? productId = null,
        int? categoryId = null,
        int? brandId = null
    )
    {
        return new Voucher(
            "SAVE10",
            discountAmount: 10,
            validFrom: DateTime.UtcNow.AddDays(-1),
            validUntil: DateTime.UtcNow.AddDays(1),
            voucherType: voucherType,
            discountType: DiscountType.Percentage,
            productId: productId,
            categoryId: categoryId,
            brandId: brandId
        );
    }
}
