using Fashia.Application.Carts.Commands.AddCartItem;
using Fashia.Application.Common.Exceptions;
using Fashia.Application.Orders.Commands.PlaceOrder;
using Fashia.Domain.Entities;
using Fashia.Domain.Enums;
using Fashia.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Fashia.Application.FunctionalTests.Orders.Commands;

public class PlaceOrderInventoryRegressionTests : TestBase
{
    [Test]
    public async Task ShouldPlaceOrderFromBranchWithAvailableStock()
    {
        var setup = await AddCheckoutSeedDataAsync();
        var destinationBranchId = await AddBranchAsync(
            "Checkout Available Branch",
            latitude: 10.2M,
            longitude: 106.2M
        );

        var reservedInventory = BranchVariantInventory.Create(
            setup.BranchId,
            setup.ProductVariantId
        );
        reservedInventory.IncreaseStock(10);
        reservedInventory.ReserveStock(8);
        await TestApp.AddAsync(reservedInventory);

        var availableInventory = BranchVariantInventory.Create(
            destinationBranchId,
            setup.ProductVariantId
        );
        availableInventory.IncreaseStock(5);
        await TestApp.AddAsync(availableInventory);

        await AddCartItemAsync(setup.ProductVariantId, quantity: 3);

        var result = await TestApp.SendAsync(CreatePlaceOrderCommand());

        var order = await TestApp.ExecuteDbContextAsync(context =>
            context.Orders.SingleAsync(x => x.Id == result.OrderId)
        );
        order.BranchId.ShouldBe(destinationBranchId);
        order.Status.ShouldBe(OrderStatus.Pending);

        var inventories = await TestApp.ExecuteDbContextAsync(context =>
            context
                .BranchVariantInventories.Where(x => x.ProductVariantId == setup.ProductVariantId)
                .OrderBy(x => x.BranchId)
                .ToListAsync()
        );

        inventories.Single(x => x.BranchId == setup.BranchId).StockQuantity.ShouldBe(10);
        inventories.Single(x => x.BranchId == setup.BranchId).ReservedQuantity.ShouldBe(8);
        inventories.Single(x => x.BranchId == destinationBranchId).StockQuantity.ShouldBe(5);
        inventories.Single(x => x.BranchId == destinationBranchId).ReservedQuantity.ShouldBe(3);

        var transaction = await TestApp.ExecuteDbContextAsync(context =>
            context.InventoryTransactions.SingleAsync(x =>
                x.Type == InventoryTransactionType.Reserve
                && x.BranchId == destinationBranchId
                && x.ProductVariantId == setup.ProductVariantId
            )
        );

        transaction.Quantity.ShouldBe(3);
        transaction.PreviousStockQuantity.ShouldBe(5);
        transaction.NewStockQuantity.ShouldBe(5);
        transaction.PreviousReservedQuantity.ShouldBe(0);
        transaction.NewReservedQuantity.ShouldBe(3);
        transaction.Note.ShouldBe("Place order reservation");
    }

    [Test]
    public async Task ShouldRejectPlaceOrderWhenOnlyReservedStockExists()
    {
        var setup = await AddCheckoutSeedDataAsync();

        var inventory = BranchVariantInventory.Create(setup.BranchId, setup.ProductVariantId);
        inventory.IncreaseStock(10);
        inventory.ReserveStock(8);
        await TestApp.AddAsync(inventory);

        await AddCartItemAsync(setup.ProductVariantId, quantity: 3);

        await Should.ThrowAsync<NoFulfillableBranchException>(() =>
            TestApp.SendAsync(CreatePlaceOrderCommand())
        );
    }

    private static async Task<CheckoutSeedData> AddCheckoutSeedDataAsync()
    {
        var userId = await TestApp.RunAsDefaultUserAsync();

        var branch = new Branch(
            "Checkout Reserved Branch",
            PhoneNumber.Create("0369405891"),
            EmailVO.Create($"{Guid.NewGuid():N}@local.test"),
            Address.Create("1 Checkout St", "Ward", "District", "Province"),
            GeoLocation.Create(10.1M, 106.1M)
        );
        await TestApp.AddAsync(branch);

        var category = new Category("Checkout Shoes");
        await TestApp.AddAsync(category);

        var brand = new Brand("Checkout Brand", "Checkout test brand");
        await TestApp.AddAsync(brand);

        var attribute = new ProductAttribute("Size");
        await TestApp.AddAsync(attribute);

        var attributeValue = new ProductAttributeValue(attribute.Id, "42");
        await TestApp.AddAsync(attributeValue);

        var productImage = CreateUploadedFile("checkout-product");
        await TestApp.AddAsync(productImage);

        var variantImage = CreateUploadedFile("checkout-variant");
        await TestApp.AddAsync(variantImage);

        var product = new Product(
            "Checkout Sneaker",
            category.Id,
            brand.Id,
            "Checkout test product",
            [productImage.Id]
        );

        var variant = product.AddVariant(Money.Create(120_000), [attributeValue.Id]);
        variant.AddImage(variantImage.Id);

        await TestApp.AddAsync(product);

        var customer = Customer.Create(
            userId,
            "Checkout",
            "Customer",
            EmailVO.Create($"{Guid.NewGuid():N}@local.test"),
            PhoneNumber.Create("0369405893")
        );
        await TestApp.AddAsync(customer);

        return new CheckoutSeedData(
            branch.Id,
            variant.Id
        );
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

    private static async Task<int> AddBranchAsync(
        string name,
        decimal latitude = 10.1M,
        decimal longitude = 106.1M
    )
    {
        var branch = new Branch(
            name,
            PhoneNumber.Create("0369405892"),
            EmailVO.Create($"{Guid.NewGuid():N}@local.test"),
            Address.Create("3 Checkout St", "Ward", "District", "Province"),
            GeoLocation.Create(latitude, longitude)
        );

        await TestApp.AddAsync(branch);

        return branch.Id;
    }

    private static async Task AddCartItemAsync(int productVariantId, int quantity)
    {
        await TestApp.SendAsync(
            new AddCartItemCommand { ProductVariantId = productVariantId, Quantity = quantity }
        );
    }

    private static PlaceOrderCommand CreatePlaceOrderCommand()
    {
        return new PlaceOrderCommand
        {
            ShippingAddress = new PlaceOrderShippingAddressDto
            {
                CustomerName = "Checkout Customer",
                CustomerEmail = "checkout@example.test",
                CustomerPhone = "0369405891",
                Line1 = "2 Checkout St",
                Ward = "Ward",
                District = "District",
                Province = "Province",
                Latitude = 10.2M,
                Longitude = 106.2M,
            },
            PaymentMethod = PaymentMethod.CashOnDelivery,
        };
    }

    private sealed record CheckoutSeedData(
        int BranchId,
        int ProductVariantId
    );
}
