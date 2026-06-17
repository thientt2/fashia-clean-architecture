using Fashia.Application.Common.Exceptions;
using Fashia.Application.Inventories.Commands.AdjustInventoryStock;
using Fashia.Application.Inventories.Commands.CommitReservedInventory;
using Fashia.Application.Inventories.Commands.DecreaseInventoryStock;
using Fashia.Application.Inventories.Commands.IncreaseInventoryStock;
using Fashia.Application.Inventories.Commands.InitializeInventory;
using Fashia.Application.Inventories.Commands.ReleaseReservedInventory;
using Fashia.Application.Inventories.Commands.ReturnInventoryStock;
using Fashia.Application.Inventories.Commands.ReserveInventoryStock;
using Fashia.Application.Inventories.Commands.TransferInventoryStock;
using Fashia.Domain.Entities;
using Fashia.Domain.Enums;
using Fashia.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Fashia.Application.FunctionalTests.Inventories.Commands;

public class InventoryStockCommandTests : TestBase
{
    [Test]
    public async Task ShouldInitializeInventory()
    {
        await TestApp.RunAsAdministratorAsync();
        var seed = await AddInventorySeedDataAsync();

        await TestApp.SendAsync(
            new InitializeInventoryCommand
            {
                BranchId = seed.BranchId,
                ProductVariantId = seed.ProductVariantId,
                Quantity = 12,
                Note = "Initial count",
            }
        );

        var inventory = await TestApp.ExecuteDbContextAsync(context =>
            context.BranchVariantInventories.SingleAsync(x =>
                x.BranchId == seed.BranchId && x.ProductVariantId == seed.ProductVariantId
            )
        );

        inventory.StockQuantity.ShouldBe(12);
        inventory.ReservedQuantity.ShouldBe(0);
        inventory.AvailableQuantity.ShouldBe(12);

        var transaction = await TestApp.ExecuteDbContextAsync(context =>
            context.InventoryTransactions.SingleAsync(x =>
                x.BranchId == seed.BranchId && x.ProductVariantId == seed.ProductVariantId
            )
        );

        transaction.Type.ShouldBe(InventoryTransactionType.Initialize);
        transaction.Quantity.ShouldBe(12);
        transaction.PreviousStockQuantity.ShouldBe(0);
        transaction.NewStockQuantity.ShouldBe(12);
        transaction.PreviousReservedQuantity.ShouldBe(0);
        transaction.NewReservedQuantity.ShouldBe(0);
        transaction.Note.ShouldBe("Initial count");
    }

    [Test]
    public async Task ShouldRejectDuplicateInitializeInventory()
    {
        await TestApp.RunAsAdministratorAsync();
        var seed = await AddInventorySeedDataAsync();

        await TestApp.SendAsync(
            new InitializeInventoryCommand
            {
                BranchId = seed.BranchId,
                ProductVariantId = seed.ProductVariantId,
                Quantity = 12,
            }
        );

        await Should.ThrowAsync<InvalidOperationException>(() =>
            TestApp.SendAsync(
                new InitializeInventoryCommand
                {
                    BranchId = seed.BranchId,
                    ProductVariantId = seed.ProductVariantId,
                    Quantity = 4,
                }
            )
        );
    }

    [Test]
    public async Task ShouldIncreaseInventoryStock()
    {
        await TestApp.RunAsAdministratorAsync();
        var seed = await AddInventorySeedDataAsync();

        await TestApp.AddAsync(new BranchVariantInventory(seed.BranchId, seed.ProductVariantId));

        await TestApp.SendAsync(
            new IncreaseInventoryStockCommand
            {
                BranchId = seed.BranchId,
                ProductVariantId = seed.ProductVariantId,
                Quantity = 5,
                Note = "Restock",
            }
        );

        var inventory = await TestApp.ExecuteDbContextAsync(context =>
            context.BranchVariantInventories.SingleAsync(x =>
                x.BranchId == seed.BranchId && x.ProductVariantId == seed.ProductVariantId
            )
        );

        inventory.StockQuantity.ShouldBe(5);
        inventory.ReservedQuantity.ShouldBe(0);

        var transaction = await TestApp.ExecuteDbContextAsync(context =>
            context.InventoryTransactions.SingleAsync(x =>
                x.BranchId == seed.BranchId && x.ProductVariantId == seed.ProductVariantId
            )
        );

        transaction.Type.ShouldBe(InventoryTransactionType.Increase);
        transaction.Quantity.ShouldBe(5);
        transaction.PreviousStockQuantity.ShouldBe(0);
        transaction.NewStockQuantity.ShouldBe(5);
        transaction.Note.ShouldBe("Restock");
    }

    [Test]
    public async Task ShouldRejectInventoryWriteForUnauthorizedUser()
    {
        await TestApp.RunAsDefaultUserAsync();
        var seed = await AddInventorySeedDataAsync();

        await Should.ThrowAsync<ForbiddenAccessException>(() =>
            TestApp.SendAsync(
                new InitializeInventoryCommand
                {
                    BranchId = seed.BranchId,
                    ProductVariantId = seed.ProductVariantId,
                    Quantity = 12,
                }
            )
        );
    }

    [Test]
    public async Task ShouldDecreaseInventoryStock()
    {
        await TestApp.RunAsAdministratorAsync();
        var seed = await AddInventorySeedDataAsync();
        var inventory = new BranchVariantInventory(seed.BranchId, seed.ProductVariantId);
        inventory.IncreaseStock(10);
        await TestApp.AddAsync(inventory);

        await TestApp.SendAsync(
            new DecreaseInventoryStockCommand
            {
                BranchId = seed.BranchId,
                ProductVariantId = seed.ProductVariantId,
                Quantity = 4,
                Note = "Damaged stock",
            }
        );

        var updatedInventory = await TestApp.ExecuteDbContextAsync(context =>
            context.BranchVariantInventories.SingleAsync(x =>
                x.BranchId == seed.BranchId && x.ProductVariantId == seed.ProductVariantId
            )
        );

        updatedInventory.StockQuantity.ShouldBe(6);
        updatedInventory.AvailableQuantity.ShouldBe(6);

        var transaction = await TestApp.ExecuteDbContextAsync(context =>
            context.InventoryTransactions.SingleAsync(x =>
                x.Type == InventoryTransactionType.Decrease
                && x.BranchId == seed.BranchId
                && x.ProductVariantId == seed.ProductVariantId
            )
        );

        transaction.Quantity.ShouldBe(4);
        transaction.PreviousStockQuantity.ShouldBe(10);
        transaction.NewStockQuantity.ShouldBe(6);
        transaction.Note.ShouldBe("Damaged stock");
    }

    [Test]
    public async Task ShouldRejectDecreaseGreaterThanAvailableStock()
    {
        await TestApp.RunAsAdministratorAsync();
        var seed = await AddInventorySeedDataAsync();
        var inventory = new BranchVariantInventory(seed.BranchId, seed.ProductVariantId);
        inventory.IncreaseStock(10);
        inventory.ReserveStock(7);
        await TestApp.AddAsync(inventory);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            TestApp.SendAsync(
                new DecreaseInventoryStockCommand
                {
                    BranchId = seed.BranchId,
                    ProductVariantId = seed.ProductVariantId,
                    Quantity = 4,
                }
            )
        );
    }

    [Test]
    public async Task ShouldAdjustInventoryStock()
    {
        await TestApp.RunAsAdministratorAsync();
        var seed = await AddInventorySeedDataAsync();
        var inventory = new BranchVariantInventory(seed.BranchId, seed.ProductVariantId);
        inventory.IncreaseStock(10);
        await TestApp.AddAsync(inventory);

        await TestApp.SendAsync(
            new AdjustInventoryStockCommand
            {
                BranchId = seed.BranchId,
                ProductVariantId = seed.ProductVariantId,
                Quantity = 6,
                Note = "Cycle count",
            }
        );

        var updatedInventory = await TestApp.ExecuteDbContextAsync(context =>
            context.BranchVariantInventories.SingleAsync(x =>
                x.BranchId == seed.BranchId && x.ProductVariantId == seed.ProductVariantId
            )
        );

        updatedInventory.StockQuantity.ShouldBe(6);

        var transaction = await TestApp.ExecuteDbContextAsync(context =>
            context.InventoryTransactions.SingleAsync(x =>
                x.Type == InventoryTransactionType.Adjustment
                && x.BranchId == seed.BranchId
                && x.ProductVariantId == seed.ProductVariantId
            )
        );

        transaction.Quantity.ShouldBe(4);
        transaction.PreviousStockQuantity.ShouldBe(10);
        transaction.NewStockQuantity.ShouldBe(6);
        transaction.Note.ShouldBe("Cycle count");
    }

    [Test]
    public async Task ShouldRejectAdjustBelowReservedStock()
    {
        await TestApp.RunAsAdministratorAsync();
        var seed = await AddInventorySeedDataAsync();
        var inventory = new BranchVariantInventory(seed.BranchId, seed.ProductVariantId);
        inventory.IncreaseStock(10);
        inventory.ReserveStock(7);
        await TestApp.AddAsync(inventory);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            TestApp.SendAsync(
                new AdjustInventoryStockCommand
                {
                    BranchId = seed.BranchId,
                    ProductVariantId = seed.ProductVariantId,
                    Quantity = 6,
                }
            )
        );
    }

    [Test]
    public async Task ShouldTransferInventoryStock()
    {
        await TestApp.RunAsAdministratorAsync();
        var seed = await AddInventorySeedDataAsync();
        var destinationBranchId = await AddBranchAsync("Destination Branch");
        var sourceInventory = new BranchVariantInventory(seed.BranchId, seed.ProductVariantId);
        sourceInventory.IncreaseStock(10);
        await TestApp.AddAsync(sourceInventory);

        await TestApp.SendAsync(
            new TransferInventoryStockCommand
            {
                SourceBranchId = seed.BranchId,
                DestinationBranchId = destinationBranchId,
                ProductVariantId = seed.ProductVariantId,
                Quantity = 4,
                Note = "Store transfer",
            }
        );

        var inventories = await TestApp.ExecuteDbContextAsync(context =>
            context
                .BranchVariantInventories.Where(x => x.ProductVariantId == seed.ProductVariantId)
                .OrderBy(x => x.BranchId)
                .ToListAsync()
        );

        inventories.Single(x => x.BranchId == seed.BranchId).StockQuantity.ShouldBe(6);
        inventories.Single(x => x.BranchId == destinationBranchId).StockQuantity.ShouldBe(4);

        var transactions = await TestApp.ExecuteDbContextAsync(context =>
            context
                .InventoryTransactions.Where(x => x.ProductVariantId == seed.ProductVariantId)
                .OrderBy(x => x.Type)
                .ToListAsync()
        );

        var transferOut = transactions.Single(x => x.Type == InventoryTransactionType.TransferOut);
        var transferIn = transactions.Single(x => x.Type == InventoryTransactionType.TransferIn);

        transferOut.BranchId.ShouldBe(seed.BranchId);
        transferOut.SourceBranchId.ShouldBe(seed.BranchId);
        transferOut.DestinationBranchId.ShouldBe(destinationBranchId);
        transferOut.PreviousStockQuantity.ShouldBe(10);
        transferOut.NewStockQuantity.ShouldBe(6);

        transferIn.BranchId.ShouldBe(destinationBranchId);
        transferIn.SourceBranchId.ShouldBe(seed.BranchId);
        transferIn.DestinationBranchId.ShouldBe(destinationBranchId);
        transferIn.PreviousStockQuantity.ShouldBe(0);
        transferIn.NewStockQuantity.ShouldBe(4);
        transferIn.TransferCorrelationId.ShouldNotBeNull();
        transferIn.TransferCorrelationId.ShouldBe(transferOut.TransferCorrelationId);
    }

    [Test]
    public async Task ShouldRejectTransferToSameBranch()
    {
        await TestApp.RunAsAdministratorAsync();
        var seed = await AddInventorySeedDataAsync();
        var sourceInventory = new BranchVariantInventory(seed.BranchId, seed.ProductVariantId);
        sourceInventory.IncreaseStock(10);
        await TestApp.AddAsync(sourceInventory);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            TestApp.SendAsync(
                new TransferInventoryStockCommand
                {
                    SourceBranchId = seed.BranchId,
                    DestinationBranchId = seed.BranchId,
                    ProductVariantId = seed.ProductVariantId,
                    Quantity = 4,
                }
            )
        );
    }

    [Test]
    public async Task ShouldRejectTransferGreaterThanAvailableStock()
    {
        await TestApp.RunAsAdministratorAsync();
        var seed = await AddInventorySeedDataAsync();
        var destinationBranchId = await AddBranchAsync("Destination Branch");
        var sourceInventory = new BranchVariantInventory(seed.BranchId, seed.ProductVariantId);
        sourceInventory.IncreaseStock(10);
        sourceInventory.ReserveStock(7);
        await TestApp.AddAsync(sourceInventory);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            TestApp.SendAsync(
                new TransferInventoryStockCommand
                {
                    SourceBranchId = seed.BranchId,
                    DestinationBranchId = destinationBranchId,
                    ProductVariantId = seed.ProductVariantId,
                    Quantity = 4,
                }
            )
        );
    }

    [Test]
    public async Task ShouldReserveInventoryStock()
    {
        await TestApp.RunAsAdministratorAsync();
        var seed = await AddInventorySeedDataAsync();
        var orderId = await AddOrderAsync(seed.BranchId, seed.ProductVariantId);
        var inventory = new BranchVariantInventory(seed.BranchId, seed.ProductVariantId);
        inventory.IncreaseStock(10);
        await TestApp.AddAsync(inventory);

        await TestApp.SendAsync(
            new ReserveInventoryStockCommand
            {
                BranchId = seed.BranchId,
                ProductVariantId = seed.ProductVariantId,
                OrderId = orderId,
                Quantity = 4,
                Note = "Order reservation",
            }
        );

        var updatedInventory = await TestApp.ExecuteDbContextAsync(context =>
            context.BranchVariantInventories.SingleAsync(x =>
                x.BranchId == seed.BranchId && x.ProductVariantId == seed.ProductVariantId
            )
        );

        updatedInventory.StockQuantity.ShouldBe(10);
        updatedInventory.ReservedQuantity.ShouldBe(4);
        updatedInventory.AvailableQuantity.ShouldBe(6);

        var transaction = await TestApp.ExecuteDbContextAsync(context =>
            context.InventoryTransactions.SingleAsync(x =>
                x.Type == InventoryTransactionType.Reserve
                && x.BranchId == seed.BranchId
                && x.ProductVariantId == seed.ProductVariantId
            )
        );

        transaction.OrderId.ShouldBe(orderId);
        transaction.Quantity.ShouldBe(4);
        transaction.PreviousStockQuantity.ShouldBe(10);
        transaction.NewStockQuantity.ShouldBe(10);
        transaction.PreviousReservedQuantity.ShouldBe(0);
        transaction.NewReservedQuantity.ShouldBe(4);
        transaction.Note.ShouldBe("Order reservation");
    }

    [Test]
    public async Task ShouldRejectReserveGreaterThanAvailableStock()
    {
        await TestApp.RunAsAdministratorAsync();
        var seed = await AddInventorySeedDataAsync();
        var orderId = await AddOrderAsync(seed.BranchId, seed.ProductVariantId);
        var inventory = new BranchVariantInventory(seed.BranchId, seed.ProductVariantId);
        inventory.IncreaseStock(10);
        inventory.ReserveStock(7);
        await TestApp.AddAsync(inventory);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            TestApp.SendAsync(
                new ReserveInventoryStockCommand
                {
                    BranchId = seed.BranchId,
                    ProductVariantId = seed.ProductVariantId,
                    OrderId = orderId,
                    Quantity = 4,
                }
            )
        );
    }

    [Test]
    public async Task ShouldReleaseReservedInventory()
    {
        await TestApp.RunAsAdministratorAsync();
        var seed = await AddInventorySeedDataAsync();
        var orderId = await AddOrderAsync(seed.BranchId, seed.ProductVariantId);
        var inventory = new BranchVariantInventory(seed.BranchId, seed.ProductVariantId);
        inventory.IncreaseStock(10);
        inventory.ReserveStock(4);
        await TestApp.AddAsync(inventory);

        await TestApp.SendAsync(
            new ReleaseReservedInventoryCommand
            {
                BranchId = seed.BranchId,
                ProductVariantId = seed.ProductVariantId,
                OrderId = orderId,
                Quantity = 3,
                Note = "Order cancelled",
            }
        );

        var updatedInventory = await TestApp.ExecuteDbContextAsync(context =>
            context.BranchVariantInventories.SingleAsync(x =>
                x.BranchId == seed.BranchId && x.ProductVariantId == seed.ProductVariantId
            )
        );

        updatedInventory.StockQuantity.ShouldBe(10);
        updatedInventory.ReservedQuantity.ShouldBe(1);
        updatedInventory.AvailableQuantity.ShouldBe(9);

        var transaction = await TestApp.ExecuteDbContextAsync(context =>
            context.InventoryTransactions.SingleAsync(x =>
                x.Type == InventoryTransactionType.ReleaseReservation
                && x.BranchId == seed.BranchId
                && x.ProductVariantId == seed.ProductVariantId
            )
        );

        transaction.OrderId.ShouldBe(orderId);
        transaction.Quantity.ShouldBe(3);
        transaction.PreviousReservedQuantity.ShouldBe(4);
        transaction.NewReservedQuantity.ShouldBe(1);
        transaction.Note.ShouldBe("Order cancelled");
    }

    [Test]
    public async Task ShouldRejectReleaseGreaterThanReservedStock()
    {
        await TestApp.RunAsAdministratorAsync();
        var seed = await AddInventorySeedDataAsync();
        var orderId = await AddOrderAsync(seed.BranchId, seed.ProductVariantId);
        var inventory = new BranchVariantInventory(seed.BranchId, seed.ProductVariantId);
        inventory.IncreaseStock(10);
        inventory.ReserveStock(2);
        await TestApp.AddAsync(inventory);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            TestApp.SendAsync(
                new ReleaseReservedInventoryCommand
                {
                    BranchId = seed.BranchId,
                    ProductVariantId = seed.ProductVariantId,
                    OrderId = orderId,
                    Quantity = 3,
                }
            )
        );
    }

    [Test]
    public async Task ShouldCommitReservedInventory()
    {
        await TestApp.RunAsAdministratorAsync();
        var seed = await AddInventorySeedDataAsync();
        var orderId = await AddOrderAsync(seed.BranchId, seed.ProductVariantId);
        var inventory = new BranchVariantInventory(seed.BranchId, seed.ProductVariantId);
        inventory.IncreaseStock(10);
        inventory.ReserveStock(4);
        await TestApp.AddAsync(inventory);

        await TestApp.SendAsync(
            new CommitReservedInventoryCommand
            {
                BranchId = seed.BranchId,
                ProductVariantId = seed.ProductVariantId,
                OrderId = orderId,
                Quantity = 3,
                Note = "Order fulfilled",
            }
        );

        var updatedInventory = await TestApp.ExecuteDbContextAsync(context =>
            context.BranchVariantInventories.SingleAsync(x =>
                x.BranchId == seed.BranchId && x.ProductVariantId == seed.ProductVariantId
            )
        );

        updatedInventory.StockQuantity.ShouldBe(7);
        updatedInventory.ReservedQuantity.ShouldBe(1);
        updatedInventory.AvailableQuantity.ShouldBe(6);

        var transaction = await TestApp.ExecuteDbContextAsync(context =>
            context.InventoryTransactions.SingleAsync(x =>
                x.Type == InventoryTransactionType.CommitReservation
                && x.BranchId == seed.BranchId
                && x.ProductVariantId == seed.ProductVariantId
            )
        );

        transaction.OrderId.ShouldBe(orderId);
        transaction.Quantity.ShouldBe(3);
        transaction.PreviousStockQuantity.ShouldBe(10);
        transaction.NewStockQuantity.ShouldBe(7);
        transaction.PreviousReservedQuantity.ShouldBe(4);
        transaction.NewReservedQuantity.ShouldBe(1);
        transaction.Note.ShouldBe("Order fulfilled");
    }

    [Test]
    public async Task ShouldRejectCommitGreaterThanReservedStock()
    {
        await TestApp.RunAsAdministratorAsync();
        var seed = await AddInventorySeedDataAsync();
        var orderId = await AddOrderAsync(seed.BranchId, seed.ProductVariantId);
        var inventory = new BranchVariantInventory(seed.BranchId, seed.ProductVariantId);
        inventory.IncreaseStock(10);
        inventory.ReserveStock(2);
        await TestApp.AddAsync(inventory);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            TestApp.SendAsync(
                new CommitReservedInventoryCommand
                {
                    BranchId = seed.BranchId,
                    ProductVariantId = seed.ProductVariantId,
                    OrderId = orderId,
                    Quantity = 3,
                }
            )
        );
    }

    [Test]
    public async Task ShouldReturnInventoryStock()
    {
        await TestApp.RunAsAdministratorAsync();
        var seed = await AddInventorySeedDataAsync();
        var orderId = await AddOrderAsync(seed.BranchId, seed.ProductVariantId);
        var inventory = new BranchVariantInventory(seed.BranchId, seed.ProductVariantId);
        inventory.IncreaseStock(10);
        inventory.ReserveStock(2);
        await TestApp.AddAsync(inventory);

        await TestApp.SendAsync(
            new ReturnInventoryStockCommand
            {
                BranchId = seed.BranchId,
                ProductVariantId = seed.ProductVariantId,
                OrderId = orderId,
                Quantity = 3,
                Note = "Customer return",
            }
        );

        var updatedInventory = await TestApp.ExecuteDbContextAsync(context =>
            context.BranchVariantInventories.SingleAsync(x =>
                x.BranchId == seed.BranchId && x.ProductVariantId == seed.ProductVariantId
            )
        );

        updatedInventory.StockQuantity.ShouldBe(13);
        updatedInventory.ReservedQuantity.ShouldBe(2);
        updatedInventory.AvailableQuantity.ShouldBe(11);

        var transaction = await TestApp.ExecuteDbContextAsync(context =>
            context.InventoryTransactions.SingleAsync(x =>
                x.Type == InventoryTransactionType.Return
                && x.BranchId == seed.BranchId
                && x.ProductVariantId == seed.ProductVariantId
            )
        );

        transaction.OrderId.ShouldBe(orderId);
        transaction.Quantity.ShouldBe(3);
        transaction.PreviousStockQuantity.ShouldBe(10);
        transaction.NewStockQuantity.ShouldBe(13);
        transaction.PreviousReservedQuantity.ShouldBe(2);
        transaction.NewReservedQuantity.ShouldBe(2);
        transaction.Note.ShouldBe("Customer return");
    }

    private static async Task<InventorySeedData> AddInventorySeedDataAsync()
    {
        var branch = new Branch(
            "Inventory Branch",
            PhoneNumber.Create("0369405891"),
            EmailVO.Create("inventory-branch@local.test"),
            Address.Create("1 Inventory St", "Ward", "District", "Province"),
            GeoLocation.Create(10.1M, 106.1M)
        );
        await TestApp.AddAsync(branch);

        var category = new Category("Inventory Shoes");
        await TestApp.AddAsync(category);

        var brand = new Brand("Inventory Brand", "Inventory test brand");
        await TestApp.AddAsync(brand);

        var attribute = new ProductAttribute("Size");
        await TestApp.AddAsync(attribute);

        var attributeValue = new ProductAttributeValue(attribute.Id, "42");
        await TestApp.AddAsync(attributeValue);

        var productImage = CreateUploadedFile("product");
        await TestApp.AddAsync(productImage);

        var variantImage = CreateUploadedFile("variant");
        await TestApp.AddAsync(variantImage);

        var product = new Product(
            "Inventory Sneaker",
            category.Id,
            brand.Id,
            "Inventory test product",
            [productImage.Id]
        );

        var variant = product.AddVariant(Money.Create(120_000), [attributeValue.Id]);
        variant.AddImage(variantImage.Id);

        await TestApp.AddAsync(product);

        return new InventorySeedData(branch.Id, variant.Id);
    }

    private static UploadedFile CreateUploadedFile(string name)
    {
        return new UploadedFile(
            $"{name}.webp",
            $"{name}.webp",
            "image/webp",
            1024,
            $"https://cdn.example.test/{name}.webp",
            $"products/{Guid.NewGuid():N}",
            "products"
        );
    }

    private static async Task<int> AddBranchAsync(string name)
    {
        var branch = new Branch(
            name,
            PhoneNumber.Create("0369405892"),
            EmailVO.Create($"{Guid.NewGuid():N}@local.test"),
            Address.Create("2 Inventory St", "Ward", "District", "Province"),
            GeoLocation.Create(10.2M, 106.2M)
        );

        await TestApp.AddAsync(branch);

        return branch.Id;
    }

    private static async Task<int> AddOrderAsync(int branchId, int productVariantId)
    {
        var userId = TestApp.GetUserId();

        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException("Test user is required.");

        var customer = Customer.Create(
            userId,
            "Inventory",
            "Customer",
            EmailVO.Create("inventory-customer@local.test"),
            PhoneNumber.Create("0369405893")
        );
        await TestApp.AddAsync(customer);

        var order = Order
            .CreateBuilder()
            .ForCustomer(
                customer.Id,
                "Inventory Customer",
                customer.CustomerEmail,
                customer.CustomerPhone
            )
            .FromBranch(branchId)
            .ShipTo(Address.Create("3 Inventory St", "Ward", "District", "Province"))
            .PaidBy(PaymentMethod.CashOnDelivery)
            .AddItem(productVariantId, Money.Create(120_000), 1)
            .Build();

        await TestApp.AddAsync(order);

        return order.Id;
    }

    private sealed record InventorySeedData(int BranchId, int ProductVariantId);
}
