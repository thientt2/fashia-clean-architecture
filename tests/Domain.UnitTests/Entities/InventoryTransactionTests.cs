using Fashia.Domain.Entities;
using Fashia.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace Fashia.Domain.UnitTests.Entities;

public class InventoryTransactionTests
{
    [Test]
    public void ShouldRecordStockSnapshot()
    {
        var transaction = new InventoryTransaction(
            branchId: 1,
            productVariantId: 2,
            type: InventoryTransactionType.Increase,
            quantity: 5,
            previousStockQuantity: 10,
            newStockQuantity: 15,
            previousReservedQuantity: 3,
            newReservedQuantity: 3,
            note: "stock received"
        );

        transaction.PreviousStockQuantity.ShouldBe(10);
        transaction.NewStockQuantity.ShouldBe(15);
        transaction.PreviousReservedQuantity.ShouldBe(3);
        transaction.NewReservedQuantity.ShouldBe(3);
        transaction.Note.ShouldBe("stock received");
    }

    [Test]
    public void ShouldRecordOrderLinkedReservation()
    {
        var transaction = new InventoryTransaction(
            branchId: 1,
            productVariantId: 2,
            type: InventoryTransactionType.Reserve,
            quantity: 4,
            previousStockQuantity: 10,
            newStockQuantity: 10,
            previousReservedQuantity: 1,
            newReservedQuantity: 5,
            orderId: 99
        );

        transaction.OrderId.ShouldBe(99);
    }

    [Test]
    public void ShouldRecordTransferMetadata()
    {
        var transferCorrelationId = Guid.NewGuid();

        var transaction = new InventoryTransaction(
            branchId: 1,
            productVariantId: 2,
            type: InventoryTransactionType.TransferOut,
            quantity: 3,
            previousStockQuantity: 10,
            newStockQuantity: 7,
            previousReservedQuantity: 2,
            newReservedQuantity: 2,
            sourceBranchId: 1,
            destinationBranchId: 5,
            transferCorrelationId: transferCorrelationId
        );

        transaction.SourceBranchId.ShouldBe(1);
        transaction.DestinationBranchId.ShouldBe(5);
        transaction.TransferCorrelationId.ShouldBe(transferCorrelationId);
    }
}
