using Fashia.Application.Carts.Commands.AddCartItem;
using Fashia.Application.Orders.Commands.CheckoutOrder;
using Fashia.Domain.Entities;
using Fashia.Domain.Enums;
using Fashia.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Fashia.Application.FunctionalTests.Orders.Commands;

public class CheckoutOrderTests : TestBase
{
    [Test]
    public async Task ShouldCheckoutFromBranchWithAvailableStock()
    {
        var setup = await AddCheckoutSeedDataAsync();
        var destinationBranchId = await AddBranchAsync("Checkout Available Branch");

        var reservedInventory = new BranchVariantInventory(
            setup.BranchId,
            setup.ProductVariantId
        );
        reservedInventory.IncreaseStock(10);
        reservedInventory.ReserveStock(8);
        await TestApp.AddAsync(reservedInventory);

        var availableInventory = new BranchVariantInventory(
            destinationBranchId,
            setup.ProductVariantId
        );
        availableInventory.IncreaseStock(5);
        await TestApp.AddAsync(availableInventory);

        var cartItemId = await AddCartItemAsync(setup.ProductVariantId, quantity: 3);

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

        var inventories = await TestApp.ExecuteDbContextAsync(context =>
            context
                .BranchVariantInventories.Where(x => x.ProductVariantId == setup.ProductVariantId)
                .OrderBy(x => x.BranchId)
                .ToListAsync()
        );

        inventories.Single(x => x.BranchId == setup.BranchId).StockQuantity.ShouldBe(10);
        inventories.Single(x => x.BranchId == setup.BranchId).ReservedQuantity.ShouldBe(8);
        inventories.Single(x => x.BranchId == destinationBranchId).StockQuantity.ShouldBe(2);

        var transaction = await TestApp.ExecuteDbContextAsync(context =>
            context.InventoryTransactions.SingleAsync(x =>
                x.Type == InventoryTransactionType.Sale
                && x.BranchId == destinationBranchId
                && x.ProductVariantId == setup.ProductVariantId
            )
        );

        transaction.Quantity.ShouldBe(3);
        transaction.PreviousStockQuantity.ShouldBe(5);
        transaction.NewStockQuantity.ShouldBe(2);
        transaction.PreviousReservedQuantity.ShouldBe(0);
        transaction.NewReservedQuantity.ShouldBe(0);
        transaction.Note.ShouldBe("Checkout order sale");
    }

    [Test]
    public async Task ShouldRejectCheckoutWhenOnlyReservedStockExists()
    {
        var setup = await AddCheckoutSeedDataAsync();

        var inventory = new BranchVariantInventory(setup.BranchId, setup.ProductVariantId);
        inventory.IncreaseStock(10);
        inventory.ReserveStock(8);
        await TestApp.AddAsync(inventory);

        var cartItemId = await AddCartItemAsync(setup.ProductVariantId, quantity: 3);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            TestApp.SendAsync(
                new CheckoutOrderCommand
                {
                    CartItemIds = [cartItemId],
                    ShippingAddressId = setup.ShippingAddressId,
                    PaymentMethod = PaymentMethod.CashOnDelivery,
                }
            )
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

        var shippingAddress = new CustomerAddress(
            customer.Id,
            "Checkout Customer",
            customer.CustomerPhone,
            Address.Create("2 Checkout St", "Ward", "District", "Province")
        );
        await TestApp.AddAsync(shippingAddress);

        return new CheckoutSeedData(
            branch.Id,
            variant.Id,
            shippingAddress.Id
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

    private static async Task<int> AddBranchAsync(string name)
    {
        var branch = new Branch(
            name,
            PhoneNumber.Create("0369405892"),
            EmailVO.Create($"{Guid.NewGuid():N}@local.test"),
            Address.Create("3 Checkout St", "Ward", "District", "Province"),
            GeoLocation.Create(10.2M, 106.2M)
        );

        await TestApp.AddAsync(branch);

        return branch.Id;
    }

    private static async Task<int> AddCartItemAsync(int productVariantId, int quantity)
    {
        var cart = await TestApp.SendAsync(
            new AddCartItemCommand { ProductVariantId = productVariantId, Quantity = quantity }
        );

        return cart.Items.Single(x => x.ProductVariantId == productVariantId).Id;
    }

    private sealed record CheckoutSeedData(
        int BranchId,
        int ProductVariantId,
        int ShippingAddressId
    );
}
