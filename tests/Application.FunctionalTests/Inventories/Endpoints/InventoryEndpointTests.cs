using Fashia.Application.Inventories.Queries.Common;
using Fashia.Domain.Entities;
using Fashia.Domain.ValueObjects;
using System.Net;
using System.Net.Http.Json;

namespace Fashia.Application.FunctionalTests.Inventories.Endpoints;

public class InventoryEndpointTests : TestBase
{
    [Test]
    public async Task EndpointShouldGetInventoryByBranch()
    {
        var seed = await AddInventoryEndpointSeedDataAsync();
        using var client = TestApp.CreateClient();

        var results = await client.GetFromJsonAsync<IReadOnlyCollection<InventoryDto>>(
            $"/api/inventories/by-branch/{seed.BranchId}"
        );

        results.ShouldNotBeNull();
        results!.Count.ShouldBe(1);
        var item = results.Single();
        item.BranchId.ShouldBe(seed.BranchId);
        item.ProductVariantId.ShouldBe(seed.ProductVariantId);
        item.StockQuantity.ShouldBe(10);
        item.ReservedQuantity.ShouldBe(2);
        item.AvailableQuantity.ShouldBe(8);
    }

    [Test]
    public async Task EndpointShouldDenyAnonymousInventoryWrites()
    {
        using var client = TestApp.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/inventories/initialize",
            new { BranchId = 1, ProductVariantId = 1, Quantity = 1 }
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task EndpointShouldNotExposeOldBranchInventoriesRoute()
    {
        using var client = TestApp.CreateClient();

        var response = await client.GetAsync("/api/branch-inventories");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private static async Task<InventoryEndpointSeedData> AddInventoryEndpointSeedDataAsync()
    {
        var branch = new Branch(
            "Endpoint Inventory Branch",
            PhoneNumber.Create("0369405891"),
            EmailVO.Create($"{Guid.NewGuid():N}@local.test"),
            Address.Create("1 Endpoint St", "Ward", "District", "Province"),
            GeoLocation.Create(10.1M, 106.1M)
        );
        await TestApp.AddAsync(branch);

        var category = new Category("Endpoint Shoes");
        await TestApp.AddAsync(category);

        var brand = new Brand("Endpoint Brand", "Endpoint test brand");
        await TestApp.AddAsync(brand);

        var attribute = new ProductAttribute("Size");
        await TestApp.AddAsync(attribute);

        var attributeValue = new ProductAttributeValue(attribute.Id, "42");
        await TestApp.AddAsync(attributeValue);

        var productImage = CreateUploadedFile("endpoint-product");
        await TestApp.AddAsync(productImage);

        var variantImage = CreateUploadedFile("endpoint-variant");
        await TestApp.AddAsync(variantImage);

        var product = new Product(
            "Endpoint Sneaker",
            category.Id,
            brand.Id,
            "Endpoint test product",
            [productImage.Id]
        );

        var variant = product.AddVariant(Money.Create(120_000), [attributeValue.Id]);
        variant.AddImage(variantImage.Id);

        await TestApp.AddAsync(product);

        var inventory = new BranchVariantInventory(branch.Id, variant.Id);
        inventory.IncreaseStock(10);
        inventory.ReserveStock(2);
        await TestApp.AddAsync(inventory);

        return new InventoryEndpointSeedData(branch.Id, variant.Id);
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

    private sealed record InventoryEndpointSeedData(int BranchId, int ProductVariantId);
}
