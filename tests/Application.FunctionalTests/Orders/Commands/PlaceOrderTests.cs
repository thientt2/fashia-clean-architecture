using Fashia.Application.Carts.Commands.AddCartItem;
using Fashia.Application.Common.Exceptions;
using Fashia.Application.Orders.Commands.PlaceOrder;
using Fashia.Domain.Entities;
using Fashia.Domain.Enums;
using Fashia.Domain.ValueObjects;
using Fashia.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fashia.Application.FunctionalTests.Orders.Commands;

public class PlaceOrderTests : TestBase
{
    [Test]
    public async Task ShouldCreatePendingOrderAtNearestFulfillableBranchAndReserveStock()
    {
        var setup = await AddPlaceOrderSeedDataAsync();
        var farBranchId = await AddBranchAsync("Far Branch", latitude: 11.0M, longitude: 107.0M);
        var nearBranchId = await AddBranchAsync("Near Branch", latitude: 10.11M, longitude: 106.11M);

        var farInventory = BranchVariantInventory.Create(farBranchId, setup.ProductVariantId);
        farInventory.IncreaseStock(5);
        await TestApp.AddAsync(farInventory);

        var nearInventory = BranchVariantInventory.Create(nearBranchId, setup.ProductVariantId);
        nearInventory.IncreaseStock(5);
        await TestApp.AddAsync(nearInventory);

        await TestApp.SendAsync(
            new AddCartItemCommand { ProductVariantId = setup.ProductVariantId, Quantity = 2 }
        );

        var result = await TestApp.SendAsync(
            new PlaceOrderCommand
            {
                ShippingAddress = new PlaceOrderShippingAddressDto
                {
                    CustomerName = "Checkout Customer",
                    CustomerEmail = "checkout@example.test",
                    CustomerPhone = "0369405891",
                    Line1 = "1 Checkout St",
                    Ward = "Ward",
                    District = "District",
                    Province = "Province",
                    Latitude = 10.10M,
                    Longitude = 106.10M,
                },
                PaymentMethod = PaymentMethod.CashOnDelivery,
            }
        );

        result.OrderId.ShouldBeGreaterThan(0);
        result.Status.ShouldBe(OrderStatus.Pending);
        result.BranchId.ShouldBe(nearBranchId);
        result.BranchName.ShouldBe("Near Branch");

        var order = await TestApp.ExecuteDbContextAsync(context =>
            context.Orders.Include(x => x.Items).SingleAsync(x => x.Id == result.OrderId)
        );

        order.BranchId.ShouldBe(nearBranchId);
        order.Items.Single().ProductName.ShouldBe("PlaceOrder Sneaker");
        order.Items.Single().VariantAttributes.ShouldBe("Size: 42");
        order.TotalAmount.Amount.ShouldBe(240_000);

        var inventories = await TestApp.ExecuteDbContextAsync(context =>
            context
                .BranchVariantInventories.Where(x => x.ProductVariantId == setup.ProductVariantId)
                .OrderBy(x => x.BranchId)
                .ToListAsync()
        );

        inventories.Single(x => x.BranchId == nearBranchId).StockQuantity.ShouldBe(5);
        inventories.Single(x => x.BranchId == nearBranchId).ReservedQuantity.ShouldBe(2);
        inventories.Single(x => x.BranchId == farBranchId).ReservedQuantity.ShouldBe(0);

        var cart = await TestApp.ExecuteDbContextAsync(context =>
            context.Carts.Include(x => x.Items).SingleAsync(x => x.CustomerId == setup.CustomerId)
        );
        cart.Items.ShouldBeEmpty();

        var transaction = await TestApp.ExecuteDbContextAsync(context =>
            context.InventoryTransactions.SingleAsync(x =>
                x.Type == InventoryTransactionType.Reserve
                && x.BranchId == nearBranchId
                && x.ProductVariantId == setup.ProductVariantId
            )
        );

        transaction.Quantity.ShouldBe(2);
        transaction.PreviousReservedQuantity.ShouldBe(0);
        transaction.NewReservedQuantity.ShouldBe(2);
    }

    [Test]
    public async Task ShouldRejectWhenNoBranchCanFulfillCart()
    {
        var setup = await AddPlaceOrderSeedDataAsync();
        var branchId = await AddBranchAsync("Low Stock Branch", latitude: 10.11M, longitude: 106.11M);
        var secondBranchId = await AddBranchAsync(
            "Second Low Stock Branch",
            latitude: 10.12M,
            longitude: 106.12M
        );

        var inventory = BranchVariantInventory.Create(branchId, setup.ProductVariantId);
        inventory.IncreaseStock(1);
        await TestApp.AddAsync(inventory);

        var secondInventory = BranchVariantInventory.Create(
            secondBranchId,
            setup.ProductVariantId
        );
        secondInventory.IncreaseStock(1);
        await TestApp.AddAsync(secondInventory);

        await TestApp.SendAsync(
            new AddCartItemCommand { ProductVariantId = setup.ProductVariantId, Quantity = 2 }
        );

        var command = new PlaceOrderCommand
        {
            ShippingAddress = new PlaceOrderShippingAddressDto
            {
                CustomerName = "Checkout Customer",
                CustomerEmail = "checkout@example.test",
                CustomerPhone = "0369405891",
                Line1 = "1 Checkout St",
                Ward = "Ward",
                District = "District",
                Province = "Province",
                Latitude = 10.10M,
                Longitude = 106.10M,
            },
            PaymentMethod = PaymentMethod.CashOnDelivery,
        };

        await Should.ThrowAsync<NoFulfillableBranchException>(() =>
            TestApp.SendAsync(
                command
            )
        );

        (await TestApp.CountAsync<IdempotencyKey>()).ShouldBe(0);
    }

    [Test]
    public async Task ShouldRejectStaleConcurrentInventoryReservation()
    {
        var setup = await AddPlaceOrderSeedDataAsync();
        var branchId = await AddBranchAsync(
            "Concurrency Branch",
            latitude: 10.11M,
            longitude: 106.11M
        );

        var inventory = BranchVariantInventory.Create(branchId, setup.ProductVariantId);
        inventory.IncreaseStock(5);
        await TestApp.AddAsync(inventory);

        using var firstScope = FunctionalTestSetup.ScopeFactory.CreateScope();
        using var secondScope = FunctionalTestSetup.ScopeFactory.CreateScope();

        var firstContext = firstScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var secondContext = secondScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var firstInventory = await firstContext.BranchVariantInventories.SingleAsync(x =>
            x.BranchId == branchId && x.ProductVariantId == setup.ProductVariantId
        );
        var secondInventory = await secondContext.BranchVariantInventories.SingleAsync(x =>
            x.BranchId == branchId && x.ProductVariantId == setup.ProductVariantId
        );

        firstInventory.ReserveStock(4);
        secondInventory.ReserveStock(4);

        await firstContext.SaveChangesAsync();

        await Should.ThrowAsync<DbUpdateConcurrencyException>(() =>
            secondContext.SaveChangesAsync()
        );

        var persistedInventory = await TestApp.ExecuteDbContextAsync(context =>
            context.BranchVariantInventories.SingleAsync(x =>
                x.BranchId == branchId && x.ProductVariantId == setup.ProductVariantId
            )
        );

        persistedInventory.ReservedQuantity.ShouldBe(4);
        persistedInventory.AvailableQuantity.ShouldBe(1);
    }

    [Test]
    public async Task ShouldApplyVoucherAndPersistSnapshot()
    {
        var setup = await AddPlaceOrderSeedDataAsync();
        var branchId = await AddBranchAsync("Voucher Branch", latitude: 10.11M, longitude: 106.11M);

        var inventory = BranchVariantInventory.Create(branchId, setup.ProductVariantId);
        inventory.IncreaseStock(5);
        await TestApp.AddAsync(inventory);

        var voucher = new Voucher(
            "SAVE30",
            discountAmount: 30_000,
            validFrom: DateTime.UtcNow.AddDays(-1),
            validUntil: DateTime.UtcNow.AddDays(1),
            voucherType: VoucherType.All
        );
        await TestApp.AddAsync(voucher);

        await TestApp.SendAsync(
            new AddCartItemCommand { ProductVariantId = setup.ProductVariantId, Quantity = 1 }
        );

        var result = await TestApp.SendAsync(CreatePlaceOrderCommand(voucher.Code));

        var order = await TestApp.ExecuteDbContextAsync(context =>
            context
                .Orders.Include(x => x.OrderVoucher)
                .SingleAsync(x => x.Id == result.OrderId)
        );

        order.DiscountAmount.Amount.ShouldBe(30_000);
        order.TotalAmount.Amount.ShouldBe(90_000);
        order.OrderVoucher.ShouldNotBeNull();
        order.OrderVoucher!.VoucherCode.ShouldBe("SAVE30");
        order.OrderVoucher.DiscountType.ShouldBe(DiscountType.FixedAmount);
        order.OrderVoucher.DiscountValue.ShouldBe(30_000);

        var persistedVoucher = await TestApp.FindAsync<Voucher>(voucher.Id);
        persistedVoucher!.UsedCount.ShouldBe(1);
    }

    [Test]
    public async Task ShouldRejectVoucherForDifferentProduct()
    {
        var setup = await AddPlaceOrderSeedDataAsync();
        var branchId = await AddBranchAsync("Voucher Branch", latitude: 10.11M, longitude: 106.11M);

        var inventory = BranchVariantInventory.Create(branchId, setup.ProductVariantId);
        inventory.IncreaseStock(5);
        await TestApp.AddAsync(inventory);

        var otherProduct = new Product(
            "Other Voucher Product",
            setup.CategoryId,
            setup.BrandId,
            "Voucher mismatch product",
            [(await AddUploadedFileAsync("other-voucher-product")).Id]
        );
        await TestApp.AddAsync(otherProduct);

        var voucher = new Voucher(
            "OTHERPRODUCT",
            discountAmount: 10,
            validFrom: DateTime.UtcNow.AddDays(-1),
            validUntil: DateTime.UtcNow.AddDays(1),
            voucherType: VoucherType.ProductSpecific,
            discountType: DiscountType.Percentage,
            productId: otherProduct.Id
        );
        await TestApp.AddAsync(voucher);

        await TestApp.SendAsync(
            new AddCartItemCommand { ProductVariantId = setup.ProductVariantId, Quantity = 1 }
        );

        await Should.ThrowAsync<InvalidVoucherException>(() =>
            TestApp.SendAsync(CreatePlaceOrderCommand(voucher.Code))
        );

        (await TestApp.CountAsync<Order>()).ShouldBe(0);
    }

    [Test]
    public async Task ShouldRejectExpiredVoucher()
    {
        var setup = await AddPlaceOrderSeedDataAsync();
        await PrepareVoucherCheckoutAsync(setup);

        var voucher = new Voucher(
            "EXPIRED",
            discountAmount: 10_000,
            validFrom: DateTime.UtcNow.AddDays(-2),
            validUntil: DateTime.UtcNow.AddDays(-1),
            voucherType: VoucherType.All
        );
        await TestApp.AddAsync(voucher);

        await Should.ThrowAsync<InvalidVoucherException>(() =>
            TestApp.SendAsync(CreatePlaceOrderCommand(voucher.Code))
        );

        (await TestApp.CountAsync<Order>()).ShouldBe(0);
    }

    [Test]
    public async Task ShouldRejectInactiveVoucher()
    {
        var setup = await AddPlaceOrderSeedDataAsync();
        await PrepareVoucherCheckoutAsync(setup);

        var voucher = new Voucher(
            "INACTIVE",
            discountAmount: 10_000,
            validFrom: DateTime.UtcNow.AddDays(-1),
            validUntil: DateTime.UtcNow.AddDays(1),
            voucherType: VoucherType.All
        );
        voucher.Deactivate();
        await TestApp.AddAsync(voucher);

        await Should.ThrowAsync<InvalidVoucherException>(() =>
            TestApp.SendAsync(CreatePlaceOrderCommand(voucher.Code))
        );

        (await TestApp.CountAsync<Order>()).ShouldBe(0);
    }

    [Test]
    public async Task ShouldRejectExhaustedVoucher()
    {
        var setup = await AddPlaceOrderSeedDataAsync();
        await PrepareVoucherCheckoutAsync(setup);

        var voucher = new Voucher(
            "EXHAUSTED",
            discountAmount: 10_000,
            validFrom: DateTime.UtcNow.AddDays(-1),
            validUntil: DateTime.UtcNow.AddDays(1),
            voucherType: VoucherType.All,
            usageLimit: 1
        );
        voucher.MarkUsed();
        await TestApp.AddAsync(voucher);

        await Should.ThrowAsync<InvalidVoucherException>(() =>
            TestApp.SendAsync(CreatePlaceOrderCommand(voucher.Code))
        );

        (await TestApp.CountAsync<Order>()).ShouldBe(0);
    }

    [Test]
    public async Task ShouldRejectPrivateVoucherWithoutCustomerAssignment()
    {
        var setup = await AddPlaceOrderSeedDataAsync();
        await PrepareVoucherCheckoutAsync(setup);

        var voucher = new Voucher(
            "PRIVATE",
            discountAmount: 10_000,
            validFrom: DateTime.UtcNow.AddDays(-1),
            validUntil: DateTime.UtcNow.AddDays(1),
            voucherType: VoucherType.All,
            display: Display.Private
        );
        await TestApp.AddAsync(voucher);

        await Should.ThrowAsync<InvalidVoucherException>(() =>
            TestApp.SendAsync(CreatePlaceOrderCommand(voucher.Code))
        );

        (await TestApp.CountAsync<Order>()).ShouldBe(0);
    }

    [Test]
    public async Task ShouldRedeemAssignedPrivateVoucher()
    {
        var setup = await AddPlaceOrderSeedDataAsync();
        await PrepareVoucherCheckoutAsync(setup);

        var voucher = new Voucher(
            "ASSIGNED",
            discountAmount: 10_000,
            validFrom: DateTime.UtcNow.AddDays(-1),
            validUntil: DateTime.UtcNow.AddDays(1),
            voucherType: VoucherType.All,
            display: Display.Private
        );
        await TestApp.AddAsync(voucher);

        var assignment = new CustomerVoucher(
            setup.CustomerId,
            voucher.Id,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1)
        );
        await TestApp.AddAsync(assignment);

        await TestApp.SendAsync(CreatePlaceOrderCommand(voucher.Code));

        var persistedAssignment = await TestApp.FindAsync<CustomerVoucher>(assignment.Id);
        persistedAssignment!.IsRedeemed.ShouldBeTrue();
        persistedAssignment.Status.ShouldBe(CustomerVoucherStatus.Inactive);
    }

    [Test]
    public async Task ShouldReturnStoredOrderForRepeatedIdempotencyKey()
    {
        var setup = await AddPlaceOrderSeedDataAsync();
        await PrepareVoucherCheckoutAsync(setup);

        var command = CreatePlaceOrderCommand() with
        {
            IdempotencyKey = Guid.NewGuid().ToString(),
        };

        var firstResponse = await TestApp.SendAsync(command);
        var retryResponse = await TestApp.SendAsync(command);

        retryResponse.ShouldBe(firstResponse);
        (await TestApp.CountAsync<Order>()).ShouldBe(1);

        var idempotencyRecord = await TestApp.ExecuteDbContextAsync(context =>
            context.IdempotencyKeys.SingleAsync(x => x.Key == command.IdempotencyKey)
        );
        idempotencyRecord.OrderId.ShouldBe(firstResponse.OrderId);
        idempotencyRecord.ResponseStatusCode.ShouldBe(201);
        idempotencyRecord.ResponseBody.ShouldNotBeNull();
        idempotencyRecord.ResponseBody!.ShouldContain(firstResponse.OrderId.ToString());

        var inventory = await TestApp.ExecuteDbContextAsync(context =>
            context.BranchVariantInventories.SingleAsync(x =>
                x.ProductVariantId == setup.ProductVariantId
            )
        );
        inventory.ReservedQuantity.ShouldBe(1);
    }

    [Test]
    public async Task ShouldRejectIdempotencyKeyReusedWithDifferentPayload()
    {
        var setup = await AddPlaceOrderSeedDataAsync();
        await PrepareVoucherCheckoutAsync(setup);
        var key = Guid.NewGuid().ToString();

        await TestApp.SendAsync(CreatePlaceOrderCommand() with { IdempotencyKey = key });

        await Should.ThrowAsync<IdempotencyKeyConflictException>(() =>
            TestApp.SendAsync(
                CreatePlaceOrderCommand() with
                {
                    IdempotencyKey = key,
                    Note = "Different checkout payload",
                }
            )
        );

        (await TestApp.CountAsync<Order>()).ShouldBe(1);
    }

    [Test]
    public async Task ShouldCreateOneOrderForConcurrentIdenticalIdempotencyKey()
    {
        var setup = await AddPlaceOrderSeedDataAsync();
        await PrepareVoucherCheckoutAsync(setup);

        var command = CreatePlaceOrderCommand() with
        {
            IdempotencyKey = Guid.NewGuid().ToString(),
        };

        var responses = await Task.WhenAll(
            TestApp.SendAsync(command),
            TestApp.SendAsync(command)
        );

        responses[0].ShouldBe(responses[1]);
        (await TestApp.CountAsync<Order>()).ShouldBe(1);

        var inventory = await TestApp.ExecuteDbContextAsync(context =>
            context.BranchVariantInventories.SingleAsync(x =>
                x.ProductVariantId == setup.ProductVariantId
            )
        );
        inventory.ReservedQuantity.ShouldBe(1);
    }

    [Test]
    public async Task ShouldRejectMatchingIdempotencyKeyWhileRequestIsInProgress()
    {
        var setup = await AddPlaceOrderSeedDataAsync();
        await PrepareVoucherCheckoutAsync(setup);

        var command = CreatePlaceOrderCommand() with
        {
            IdempotencyKey = Guid.NewGuid().ToString(),
        };
        var now = DateTime.UtcNow;
        var record = IdempotencyKey.Create(
            command.IdempotencyKey,
            setup.CustomerId,
            ComputeRequestHash(command),
            now,
            now.AddHours(24)
        );
        await TestApp.AddAsync(record);

        await Should.ThrowAsync<DuplicateRequestInProgressException>(() =>
            TestApp.SendAsync(command)
        );

        (await TestApp.CountAsync<Order>()).ShouldBe(0);
    }

    private static async Task<PlaceOrderSeedData> AddPlaceOrderSeedDataAsync()
    {
        var userId = await TestApp.RunAsDefaultUserAsync();

        var category = new Category("PlaceOrder Shoes");
        await TestApp.AddAsync(category);

        var brand = new Brand("PlaceOrder Brand", "PlaceOrder test brand");
        await TestApp.AddAsync(brand);

        var attribute = new ProductAttribute("Size");
        await TestApp.AddAsync(attribute);

        var attributeValue = new ProductAttributeValue(attribute.Id, "42");
        await TestApp.AddAsync(attributeValue);

        var productImage = CreateUploadedFile("placeorder-product");
        await TestApp.AddAsync(productImage);

        var variantImage = CreateUploadedFile("placeorder-variant");
        await TestApp.AddAsync(variantImage);

        var product = new Product(
            "PlaceOrder Sneaker",
            category.Id,
            brand.Id,
            "PlaceOrder test product",
            [productImage.Id]
        );

        var variant = product.AddVariant(Money.Create(120_000), [attributeValue.Id]);
        variant.AddImage(variantImage.Id);

        await TestApp.AddAsync(product);

        var customer = Customer.Create(
            userId,
            "Checkout",
            "Customer",
            EmailVO.Create("checkout@example.test"),
            PhoneNumber.Create("0369405891")
        );
        await TestApp.AddAsync(customer);

        return new PlaceOrderSeedData(
            customer.Id,
            product.Id,
            category.Id,
            brand.Id,
            variant.Id
        );
    }

    private static async Task<int> AddBranchAsync(
        string name,
        decimal latitude,
        decimal longitude
    )
    {
        var branch = new Branch(
            name,
            PhoneNumber.Create("0369405892"),
            EmailVO.Create($"{Guid.NewGuid():N}@local.test"),
            Address.Create("2 Checkout St", "Ward", "District", "Province"),
            GeoLocation.Create(latitude, longitude)
        );

        await TestApp.AddAsync(branch);

        return branch.Id;
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

    private static async Task<UploadedFile> AddUploadedFileAsync(string name)
    {
        var file = CreateUploadedFile(name);
        await TestApp.AddAsync(file);
        return file;
    }

    private static async Task PrepareVoucherCheckoutAsync(PlaceOrderSeedData setup)
    {
        var branchId = await AddBranchAsync(
            $"Voucher Branch {Guid.NewGuid():N}",
            latitude: 10.11M,
            longitude: 106.11M
        );

        var inventory = BranchVariantInventory.Create(branchId, setup.ProductVariantId);
        inventory.IncreaseStock(5);
        await TestApp.AddAsync(inventory);

        await TestApp.SendAsync(
            new AddCartItemCommand { ProductVariantId = setup.ProductVariantId, Quantity = 1 }
        );
    }

    private static PlaceOrderCommand CreatePlaceOrderCommand(string? voucherCode = null)
    {
        return new PlaceOrderCommand
        {
            ShippingAddress = new PlaceOrderShippingAddressDto
            {
                CustomerName = "Checkout Customer",
                CustomerEmail = "checkout@example.test",
                CustomerPhone = "0369405891",
                Line1 = "1 Checkout St",
                Ward = "Ward",
                District = "District",
                Province = "Province",
                Latitude = 10.10M,
                Longitude = 106.10M,
            },
            VoucherCode = voucherCode,
            PaymentMethod = PaymentMethod.CashOnDelivery,
        };
    }

    private static string ComputeRequestHash(PlaceOrderCommand command)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
        var normalizedRequest = JsonSerializer.Serialize(command, options);

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalizedRequest)));
    }

    private sealed record PlaceOrderSeedData(
        int CustomerId,
        int ProductId,
        int CategoryId,
        int BrandId,
        int ProductVariantId
    );
}
