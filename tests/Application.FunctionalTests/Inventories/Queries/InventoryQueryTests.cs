using Fashia.Application.Inventories.Queries.GetInventoryByBranch;
using Fashia.Application.Inventories.Queries.GetInventoryByProduct;
using Fashia.Application.Inventories.Queries.GetInventoryTransactionHistory;
using Fashia.Application.Inventories.Queries.GetLowStockInventory;
using Fashia.Domain.Entities;
using Fashia.Domain.Enums;
using Fashia.Domain.ValueObjects;

namespace Fashia.Application.FunctionalTests.Inventories.Queries;

public class InventoryQueryTests : TestBase
{
    [Test]
    public async Task ShouldGetInventoryByBranch()
    {
        var seed = await AddInventoryQuerySeedDataAsync();

        var results = await TestApp.SendAsync(new GetInventoryByBranchQuery(seed.BranchId));

        results.Count.ShouldBe(2);
        results.ShouldAllBe(x => x.BranchId == seed.BranchId);
        results.Select(x => x.ProductVariantId).ShouldBe([seed.FirstVariantId, seed.SecondVariantId]);

        var firstVariant = results.Single(x => x.ProductVariantId == seed.FirstVariantId);
        firstVariant.BranchName.ShouldBe("Inventory Query Branch");
        firstVariant.ProductId.ShouldBe(seed.ProductId);
        firstVariant.ProductName.ShouldBe("Inventory Query Sneaker");
        firstVariant.StockQuantity.ShouldBe(10);
        firstVariant.ReservedQuantity.ShouldBe(3);
        firstVariant.AvailableQuantity.ShouldBe(7);
        firstVariant.SellingPrice.ShouldBe(120_000);
        firstVariant.AttributeValues.Single().Value.ShouldBe("42");
    }

    [Test]
    public async Task ShouldGetInventoryByProductAcrossBranches()
    {
        var seed = await AddInventoryQuerySeedDataAsync();

        var results = await TestApp.SendAsync(new GetInventoryByProductQuery(seed.ProductId));

        results.Count.ShouldBe(3);
        results.Select(x => x.BranchId).Distinct().Order().ShouldBe(
            [seed.BranchId, seed.SecondBranchId]
        );
        results.ShouldAllBe(x => x.ProductId == seed.ProductId);

        var secondBranchInventory = results.Single(x =>
            x.BranchId == seed.SecondBranchId && x.ProductVariantId == seed.FirstVariantId
        );
        secondBranchInventory.BranchName.ShouldBe("Inventory Query Second Branch");
        secondBranchInventory.StockQuantity.ShouldBe(4);
        secondBranchInventory.ReservedQuantity.ShouldBe(1);
        secondBranchInventory.AvailableQuantity.ShouldBe(3);
    }

    [Test]
    public async Task ShouldGetLowStockInventory()
    {
        var seed = await AddInventoryQuerySeedDataAsync();

        var results = await TestApp.SendAsync(new GetLowStockInventoryQuery());

        results.Count.ShouldBe(2);
        results.ShouldAllBe(x => x.AvailableQuantity <= 10);
        results.Select(x => x.ProductVariantId).ShouldBe([seed.FirstVariantId, seed.FirstVariantId]);
        results.Select(x => x.BranchId).ShouldBe([seed.BranchId, seed.SecondBranchId]);
    }

    [Test]
    public async Task ShouldGetPagedInventoryTransactionHistoryWithFilters()
    {
        var seed = await AddInventoryQuerySeedDataAsync();
        await AddInventoryTransactionAsync(
            seed.BranchId,
            seed.FirstVariantId,
            InventoryTransactionType.Increase,
            quantity: 10,
            orderId: null,
            note: "Old restock"
        );
        await AddInventoryTransactionAsync(
            seed.BranchId,
            seed.FirstVariantId,
            InventoryTransactionType.Reserve,
            quantity: 3,
            orderId: 123,
            note: "First reservation"
        );
        await AddInventoryTransactionAsync(
            seed.BranchId,
            seed.FirstVariantId,
            InventoryTransactionType.Reserve,
            quantity: 2,
            orderId: 123,
            note: "Second reservation"
        );
        await AddInventoryTransactionAsync(
            seed.SecondBranchId,
            seed.FirstVariantId,
            InventoryTransactionType.Reserve,
            quantity: 1,
            orderId: 123,
            note: "Other branch reservation"
        );

        var result = await TestApp.SendAsync(
            new GetInventoryTransactionHistoryQuery
            {
                BranchId = seed.BranchId,
                ProductVariantId = seed.FirstVariantId,
                Type = InventoryTransactionType.Reserve,
                OrderId = 123,
                PageNumber = 1,
                PageSize = 1,
            }
        );

        result.PageNumber.ShouldBe(1);
        result.PageSize.ShouldBe(1);
        result.TotalCount.ShouldBe(2);
        result.Items.Count.ShouldBe(1);

        var item = result.Items.Single();
        item.BranchId.ShouldBe(seed.BranchId);
        item.ProductVariantId.ShouldBe(seed.FirstVariantId);
        item.Type.ShouldBe(InventoryTransactionType.Reserve.ToString());
        item.Quantity.ShouldBe(2);
        item.OrderId.ShouldBe(123);
        item.Note.ShouldBe("Second reservation");
    }

    private static async Task<InventoryQuerySeedData> AddInventoryQuerySeedDataAsync()
    {
        var branch = new Branch(
            "Inventory Query Branch",
            PhoneNumber.Create("0369405891"),
            EmailVO.Create($"{Guid.NewGuid():N}@local.test"),
            Address.Create("1 Inventory Query St", "Ward", "District", "Province"),
            GeoLocation.Create(10.1M, 106.1M)
        );
        await TestApp.AddAsync(branch);

        var secondBranch = new Branch(
            "Inventory Query Second Branch",
            PhoneNumber.Create("0369405892"),
            EmailVO.Create($"{Guid.NewGuid():N}@local.test"),
            Address.Create("2 Inventory Query St", "Ward", "District", "Province"),
            GeoLocation.Create(10.2M, 106.2M)
        );
        await TestApp.AddAsync(secondBranch);

        var category = new Category("Inventory Query Shoes");
        await TestApp.AddAsync(category);

        var brand = new Brand("Inventory Query Brand", "Inventory query test brand");
        await TestApp.AddAsync(brand);

        var attribute = new ProductAttribute("Size");
        await TestApp.AddAsync(attribute);

        var firstAttributeValue = new ProductAttributeValue(attribute.Id, "42");
        await TestApp.AddAsync(firstAttributeValue);

        var secondAttributeValue = new ProductAttributeValue(attribute.Id, "43");
        await TestApp.AddAsync(secondAttributeValue);

        var productImage = CreateUploadedFile("inventory-query-product");
        await TestApp.AddAsync(productImage);

        var firstVariantImage = CreateUploadedFile("inventory-query-first-variant");
        await TestApp.AddAsync(firstVariantImage);

        var secondVariantImage = CreateUploadedFile("inventory-query-second-variant");
        await TestApp.AddAsync(secondVariantImage);

        var product = new Product(
            "Inventory Query Sneaker",
            category.Id,
            brand.Id,
            "Inventory query test product",
            [productImage.Id]
        );

        var firstVariant = product.AddVariant(Money.Create(120_000), [firstAttributeValue.Id]);
        firstVariant.AddImage(firstVariantImage.Id);

        var secondVariant = product.AddVariant(Money.Create(130_000), [secondAttributeValue.Id]);
        secondVariant.AddImage(secondVariantImage.Id);

        await TestApp.AddAsync(product);

        var firstInventory = new BranchVariantInventory(branch.Id, firstVariant.Id);
        firstInventory.IncreaseStock(10);
        firstInventory.ReserveStock(3);
        await TestApp.AddAsync(firstInventory);

        var secondInventory = new BranchVariantInventory(branch.Id, secondVariant.Id);
        secondInventory.IncreaseStock(14);
        await TestApp.AddAsync(secondInventory);

        var secondBranchInventory = new BranchVariantInventory(secondBranch.Id, firstVariant.Id);
        secondBranchInventory.IncreaseStock(4);
        secondBranchInventory.ReserveStock(1);
        await TestApp.AddAsync(secondBranchInventory);

        return new InventoryQuerySeedData(
            branch.Id,
            secondBranch.Id,
            product.Id,
            firstVariant.Id,
            secondVariant.Id
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

    private static async Task AddInventoryTransactionAsync(
        int branchId,
        int productVariantId,
        InventoryTransactionType type,
        int quantity,
        int? orderId,
        string note
    )
    {
        await TestApp.AddAsync(
            new InventoryTransaction(
                branchId,
                productVariantId,
                type,
                quantity,
                note,
                previousStockQuantity: 10,
                newStockQuantity: 10,
                previousReservedQuantity: 0,
                newReservedQuantity: quantity,
                orderId: orderId
            )
        );
    }

    private sealed record InventoryQuerySeedData(
        int BranchId,
        int SecondBranchId,
        int ProductId,
        int FirstVariantId,
        int SecondVariantId
    );
}
