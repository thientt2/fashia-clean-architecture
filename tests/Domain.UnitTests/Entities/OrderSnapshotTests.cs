using Fashia.Domain.Entities;
using Fashia.Domain.Enums;
using Fashia.Domain.ValueObjects;
using NUnit.Framework;
using Shouldly;

namespace Fashia.Domain.UnitTests.Entities;

public class OrderSnapshotTests
{
    [Test]
    public void ShouldCreateOrderItemSnapshot()
    {
        var item = OrderItem.CreateSnapshot(
            productVariantId: 12,
            productName: "Runner",
            productVariantName: "Runner / Red / 42",
            variantName: "Red / 42",
            variantAttributes: "Color: Red; Size: 42",
            sku: "RUN-RED-42",
            unitPrice: Money.Create(120_000),
            quantity: 2
        );

        item.ProductVariantId.ShouldBe(12);
        item.ProductName.ShouldBe("Runner");
        item.ProductVariantName.ShouldBe("Runner / Red / 42");
        item.VariantName.ShouldBe("Red / 42");
        item.VariantAttributes.ShouldBe("Color: Red; Size: 42");
        item.Sku.ShouldBe("RUN-RED-42");
        item.UnitPrice.Amount.ShouldBe(120_000);
        item.Quantity.ShouldBe(2);
        item.LineTotal.Amount.ShouldBe(240_000);
    }

    [Test]
    public void ShouldCreateOrderVoucherSnapshot()
    {
        var voucher = new OrderVoucher(
            voucherId: 4,
            voucherCode: "SUMMER",
            discountType: DiscountType.Percentage,
            discountValue: 15,
            discountAmount: Money.Create(30_000)
        );

        voucher.VoucherId.ShouldBe(4);
        voucher.VoucherCode.ShouldBe("SUMMER");
        voucher.DiscountType.ShouldBe(DiscountType.Percentage);
        voucher.DiscountValue.ShouldBe(15);
        voucher.DiscountAmount.Amount.ShouldBe(30_000);
    }
}
