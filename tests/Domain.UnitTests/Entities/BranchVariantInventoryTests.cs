using Fashia.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace Fashia.Domain.UnitTests.Entities;

public class BranchVariantInventoryTests
{
    [Test]
    public void ShouldTrackAvailableStockSeparatelyFromReservedStock()
    {
        var inventory = BranchVariantInventory.Create(branchId: 1, variantId: 2);

        inventory.IncreaseStock(10);
        inventory.ReserveStock(4);

        inventory.StockQuantity.ShouldBe(10);
        inventory.ReservedQuantity.ShouldBe(4);
        inventory.AvailableQuantity.ShouldBe(6);
    }

    [Test]
    public void ShouldRejectReservationGreaterThanAvailableStock()
    {
        var inventory = BranchVariantInventory.Create(branchId: 1, variantId: 2);
        inventory.IncreaseStock(5);
        inventory.ReserveStock(3);

        Should.Throw<InvalidOperationException>(() => inventory.ReserveStock(3));
    }

    [Test]
    public void ShouldRejectDecreaseGreaterThanAvailableStock()
    {
        var inventory = BranchVariantInventory.Create(branchId: 1, variantId: 2);
        inventory.IncreaseStock(10);
        inventory.ReserveStock(7);

        Should.Throw<InvalidOperationException>(() => inventory.DecreaseStock(4));
    }

    [Test]
    public void ShouldRejectReleaseGreaterThanReservedStock()
    {
        var inventory = BranchVariantInventory.Create(branchId: 1, variantId: 2);
        inventory.IncreaseStock(10);
        inventory.ReserveStock(3);

        Should.Throw<InvalidOperationException>(() => inventory.ReleaseReservedStock(4));
    }

    [Test]
    public void ShouldCommitReservedStock()
    {
        var inventory = BranchVariantInventory.Create(branchId: 1, variantId: 2);
        inventory.IncreaseStock(10);
        inventory.ReserveStock(4);

        inventory.CommitReservedStock(3);

        inventory.StockQuantity.ShouldBe(7);
        inventory.ReservedQuantity.ShouldBe(1);
        inventory.AvailableQuantity.ShouldBe(6);
    }

    [Test]
    public void ShouldRejectAdjustBelowReservedStock()
    {
        var inventory = BranchVariantInventory.Create(branchId: 1, variantId: 2);
        inventory.IncreaseStock(10);
        inventory.ReserveStock(6);

        Should.Throw<InvalidOperationException>(() => inventory.AdjustStock(5));
    }

    [Test]
    public void ShouldReturnStockWithoutChangingReservedStock()
    {
        var inventory = BranchVariantInventory.Create(branchId: 1, variantId: 2);
        inventory.IncreaseStock(10);
        inventory.ReserveStock(3);

        inventory.ReturnStock(2);

        inventory.StockQuantity.ShouldBe(12);
        inventory.ReservedQuantity.ShouldBe(3);
        inventory.AvailableQuantity.ShouldBe(9);
    }
}
