using Fashia.Application.Carts.Commands.AddCartItem;
using Fashia.Application.Inventories.Commands.InitializeInventory;
using Fashia.Application.Inventories.Commands.ReserveInventoryStock;
using Fashia.Application.Inventories.Queries.GetInventoryByBranch;
using Fashia.Application.Inventories.Queries.GetInventoryTransactionHistory;
using Fashia.Application.Orders.Commands.CheckoutOrder;
using Fashia.Domain.Entities;
using Fashia.Domain.Enums;
using Fashia.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Fashia.Application.FunctionalTests.Inventories;

public class InventoryEndToEndTests : TestBase
{
    [Test]
    public async Task ShouldShowStockChangingOperationInTransactionHistory()
    {
        await TestApp.RunAsAdministratorAsync();
        var seed = await AddInventorySeedDataAsync();

        await TestApp.SendAsync(
            new InitializeInventoryCommand
            {
                BranchId = seed.BranchId,
                ProductVariantId = seed.ProductVariantId,
                Quantity = 10,
                Note = "Opening count",
            }
        );

        var history = await TestApp.SendAsync(
            new GetInventoryTransactionHistoryQuery
            {
                BranchId = seed.BranchId,
                ProductVariantId = seed.ProductVariantId,
                Type = InventoryTransactionType.Initialize,
                PageNumber = 1,
                PageSize = 10,
            }
        );

        history.TotalCount.ShouldBe(1);
        var transaction = history.Items.Single();
        transaction.Type.ShouldBe(InventoryTransactionType.Initialize.ToString());
        transaction.Quantity.ShouldBe(10);
        transaction.PreviousStockQuantity.ShouldBe(0);
        transaction.NewStockQuantity.ShouldBe(10);
        transaction.Note.ShouldBe("Opening count");
    }

    [Test]
    public async Task ShouldReflectReservationLifecycleInInventoryQueries()
    {
        await TestApp.RunAsAdministratorAsync();
        var seed = await AddInventorySeedDataAsync();
        var orderId = await AddOrderAsync(seed.BranchId, seed.ProductVariantId);

        await TestApp.SendAsync(
            new InitializeInventoryCommand
            {
                BranchId = seed.BranchId,
                ProductVariantId = seed.ProductVariantId,
                Quantity = 10,
            }
        );

        await TestApp.SendAsync(
            new ReserveInventoryStockCommand
            {
                BranchId = seed.BranchId,
                ProductVariantId = seed.ProductVariantId,
                OrderId = orderId,
                Quantity = 4,
                Note = "Reserve for order",
            }
        );

        var branchInventory = await TestApp.SendAsync(
            new GetInventoryByBranchQuery(seed.BranchId)
        );

        var inventory = branchInventory.Single(x => x.ProductVariantId == seed.ProductVariantId);
        inventory.StockQuantity.ShouldBe(10);
        inventory.ReservedQuantity.ShouldBe(4);
        inventory.AvailableQuantity.ShouldBe(6);

        var history = await TestApp.SendAsync(
            new GetInventoryTransactionHistoryQuery
            {
                BranchId = seed.BranchId,
                ProductVariantId = seed.ProductVariantId,
                Type = InventoryTransactionType.Reserve,
                OrderId = orderId,
                PageNumber = 1,
                PageSize = 10,
            }
        );

        history.TotalCount.ShouldBe(1);
        history.Items.Single().NewReservedQuantity.ShouldBe(4);
    }

    [Test]
    public async Task ShouldPreserveInventoryInvariantsWhenCheckoutUsesAvailableStock()
    {
        var setup = await AddCheckoutSeedDataAsync();
        var destinationBranchId = await AddBranchAsync("Checkout Destination Branch");

        var reservedInventory = new BranchVariantInventory(
            setup.BranchId,
            setup.ProductVariantId
        );
        reservedInventory.IncreaseStock(10);
        reservedInventory.ReserveStock(8);
        await TestApp.AddAsync(reservedInventory);

        var destinationInventory = new BranchVariantInventory(
            destinationBranchId,
            setup.ProductVariantId
        );
        destinationInventory.IncreaseStock(5);
        await TestApp.AddAsync(destinationInventory);

        var cart = await TestApp.SendAsync(
            new AddCartItemCommand { ProductVariantId = setup.ProductVariantId, Quantity = 3 }
        );
        var cartItemId = cart.Items.Single().Id;

        var orderId = await TestApp.SendAsync(
            new CheckoutOrderCommand
            {
                CartItemIds = [cartItemId],
                ShippingAddressId = setup.ShippingAddressId,
                PaymentMethod = PaymentMethod.CashOnDelivery,
            }
        );

        var order = await TestApp.ExecuteDbContextAsync(context =>
            context.Orders.SingleAsync(x => x.Id == orderId)
        );
        order.BranchId.ShouldBe(destinationBranchId);

        var sourceInventoryView = await TestApp.SendAsync(
            new GetInventoryByBranchQuery(setup.BranchId)
        );
        var destinationInventoryView = await TestApp.SendAsync(
            new GetInventoryByBranchQuery(destinationBranchId)
        );

        var source = sourceInventoryView.Single(x =>
            x.ProductVariantId == setup.ProductVariantId
        );
        source.StockQuantity.ShouldBe(10);
        source.ReservedQuantity.ShouldBe(8);
        source.AvailableQuantity.ShouldBe(2);

        var destination = destinationInventoryView.Single(x =>
            x.ProductVariantId == setup.ProductVariantId
        );
        destination.StockQuantity.ShouldBe(2);
        destination.ReservedQuantity.ShouldBe(0);
        destination.AvailableQuantity.ShouldBe(2);

        var saleHistory = await TestApp.SendAsync(
            new GetInventoryTransactionHistoryQuery
            {
                BranchId = destinationBranchId,
                ProductVariantId = setup.ProductVariantId,
                Type = InventoryTransactionType.Sale,
                PageNumber = 1,
                PageSize = 10,
            }
        );

        saleHistory.TotalCount.ShouldBe(1);
        var sale = saleHistory.Items.Single();
        sale.Quantity.ShouldBe(3);
        sale.PreviousStockQuantity.ShouldBe(5);
        sale.NewStockQuantity.ShouldBe(2);
        sale.PreviousReservedQuantity.ShouldBe(0);
        sale.NewReservedQuantity.ShouldBe(0);
    }

    private static async Task<InventorySeedData> AddInventorySeedDataAsync()
    {
        var branch = await AddBranchAsync("Inventory End To End Branch");
        var variant = await AddProductVariantAsync("Inventory End To End Sneaker");

        return new InventorySeedData(branch, variant);
    }

    private static async Task<CheckoutSeedData> AddCheckoutSeedDataAsync()
    {
        var userId = await TestApp.RunAsDefaultUserAsync();
        var branchId = await AddBranchAsync("Checkout Source Branch");
        var variantId = await AddProductVariantAsync("Checkout End To End Sneaker");

        var customer = Customer.Create(
            userId,
            "Checkout",
            "Customer",
            EmailVO.Create($"{Guid.NewGuid():N}@local.test"),
            PhoneNumber.Create("0369405893")
        );
        await TestApp.AddAsync(customer);

        var shippingAddress = new CustomerAddress(
            customer.Id,
            "Checkout Customer",
            customer.CustomerPhone,
            Address.Create("3 Checkout St", "Ward", "District", "Province")
        );
        await TestApp.AddAsync(shippingAddress);

        return new CheckoutSeedData(branchId, variantId, shippingAddress.Id);
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
            EmailVO.Create($"{Guid.NewGuid():N}@local.test"),
            PhoneNumber.Create("0369405894")
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
            .ShipTo(Address.Create("4 Inventory St", "Ward", "District", "Province"))
            .PaidBy(PaymentMethod.CashOnDelivery)
            .AddItem(productVariantId, Money.Create(120_000), 1)
            .Build();

        await TestApp.AddAsync(order);

        return order.Id;
    }

    private static async Task<int> AddBranchAsync(string name)
    {
        var branch = new Branch(
            name,
            PhoneNumber.Create("0369405891"),
            EmailVO.Create($"{Guid.NewGuid():N}@local.test"),
            Address.Create("1 Inventory St", "Ward", "District", "Province"),
            GeoLocation.Create(10.1M, 106.1M)
        );
        await TestApp.AddAsync(branch);

        return branch.Id;
    }

    private static async Task<int> AddProductVariantAsync(string productName)
    {
        var category = new Category($"{productName} Category");
        await TestApp.AddAsync(category);

        var brand = new Brand($"{productName} Brand", "Inventory end to end brand");
        await TestApp.AddAsync(brand);

        var attribute = new ProductAttribute("Size");
        await TestApp.AddAsync(attribute);

        var attributeValue = new ProductAttributeValue(attribute.Id, "42");
        await TestApp.AddAsync(attributeValue);

        var productImage = CreateUploadedFile($"{productName}-product");
        await TestApp.AddAsync(productImage);

        var variantImage = CreateUploadedFile($"{productName}-variant");
        await TestApp.AddAsync(variantImage);

        var product = new Product(
            productName,
            category.Id,
            brand.Id,
            "Inventory end to end product",
            [productImage.Id]
        );

        var variant = product.AddVariant(Money.Create(120_000), [attributeValue.Id]);
        variant.AddImage(variantImage.Id);

        await TestApp.AddAsync(product);

        return variant.Id;
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

    private sealed record InventorySeedData(int BranchId, int ProductVariantId);

    private sealed record CheckoutSeedData(
        int BranchId,
        int ProductVariantId,
        int ShippingAddressId
    );
}
