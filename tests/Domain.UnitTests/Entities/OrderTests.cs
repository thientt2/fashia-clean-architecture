using Fashia.Domain.Entities;
using Fashia.Domain.Enums;
using Fashia.Domain.Events;
using Fashia.Domain.ValueObjects;
using NUnit.Framework;
using Shouldly;

namespace Fashia.Domain.UnitTests.Entities;

public class OrderTests
{
    [Test]
    public void ShouldBuildPendingOrderWithItemSnapshots()
    {
        var order = BuildOrder();

        order.Status.ShouldBe(OrderStatus.Pending);
        order.Items.Count.ShouldBe(1);

        var item = order.Items.Single();
        item.ProductName.ShouldBe("Runner");
        item.ProductVariantName.ShouldBe("Runner / Red / 42");
        item.VariantAttributes.ShouldBe("Color: Red; Size: 42");
        order.TotalAmount.Amount.ShouldBe(240_000);
    }

    [Test]
    public void ShouldRaiseOrderPlacedEventWhenBuilt()
    {
        var order = BuildOrder();

        var domainEvent = order.DomainEvents.OfType<OrderPlacedEvent>().Single();
        domainEvent.Order.ShouldBe(order);
    }

    [Test]
    public void ShouldRejectEmptyOrder()
    {
        Should.Throw<InvalidOperationException>(() =>
            Order
                .CreateBuilder()
                .ForCustomer(
                    customerId: 1,
                    customerName: "Test Customer",
                    customerEmail: EmailVO.Create("customer@example.test"),
                    customerPhone: PhoneNumber.Create("0369405891")
                )
                .FromBranch(2)
                .ShipTo(Address.Create("1 Main St", "Ward", "District", "Province"))
                .Build()
        );
    }

    private static Order BuildOrder()
    {
        return Order
            .CreateBuilder()
            .ForCustomer(
                customerId: 1,
                customerName: "Test Customer",
                customerEmail: EmailVO.Create("customer@example.test"),
                customerPhone: PhoneNumber.Create("0369405891")
            )
            .FromBranch(2)
            .ShipTo(Address.Create("1 Main St", "Ward", "District", "Province"))
            .PaidBy(PaymentMethod.CashOnDelivery)
            .AddItemSnapshot(
                productVariantId: 12,
                productName: "Runner",
                productVariantName: "Runner / Red / 42",
                variantName: "Red / 42",
                variantAttributes: "Color: Red; Size: 42",
                sku: "RUN-RED-42",
                unitPrice: Money.Create(120_000),
                quantity: 2
            )
            .Build();
    }
}
