using Fashia.Application.Common.Exceptions;
using Fashia.Application.Products.Commands.CreateProduct;
using Fashia.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;

namespace Fashia.Application.FunctionalTests.Products.Commands;

public class CreateProductTests : TestBase
{
    [Test]
    public async Task ShouldCreateProduct()
    {
        await TestApp.RunAsDefaultUserAsync();
        var seed = await AddProductSeedDataAsync();

        var command = CreateValidCommand(seed);

        var productId = await TestApp.SendAsync(command);

        var product = await TestApp.ExecuteDbContextAsync(context =>
            context.Products
                .Include(x => x.Images)
                .Include(x => x.Variants)
                    .ThenInclude(x => x.Images)
                .Include(x => x.Variants)
                    .ThenInclude(x => x.AttributeValues)
                .SingleAsync(x => x.Id == productId)
        );

        product.Name.ShouldBe(command.Name);
        product.Description.ShouldBe(command.Description);
        product.CategoryId.ShouldBe(seed.CategoryId);
        product.BrandId.ShouldBe(seed.BrandId);
        product.Images.Count.ShouldBe(1);
        product.Variants.Count.ShouldBe(1);
        product.Variants.Single().OriginalPrice.Amount.ShouldBe(120_000);
        product.Variants.Single().Images.Count.ShouldBe(1);
        product.Variants.Single().AttributeValues.Count.ShouldBe(1);
    }

    [Test]
    public async Task ShouldRequireName()
    {
        await TestApp.RunAsDefaultUserAsync();
        var seed = await AddProductSeedDataAsync();
        var command = CreateValidCommand(seed) with { Name = string.Empty };

        var exception = await Should.ThrowAsync<ValidationException>(() =>
            TestApp.SendAsync(command)
        );

        exception.Errors.ShouldContainKey("Name");
    }

    [Test]
    public async Task ShouldLimitNameLength()
    {
        await TestApp.RunAsDefaultUserAsync();
        var seed = await AddProductSeedDataAsync();
        var command = CreateValidCommand(seed) with { Name = new string('a', 201) };

        var exception = await Should.ThrowAsync<ValidationException>(() =>
            TestApp.SendAsync(command)
        );

        exception.Errors.ShouldContainKey("Name");
    }

    [Test]
    public async Task ShouldRequireOriginalPriceGreaterThanZero()
    {
        await TestApp.RunAsDefaultUserAsync();
        var seed = await AddProductSeedDataAsync();
        var command = CreateValidCommand(seed) with
        {
            Variants =
            [
                new CreateProductVariantDto
                {
                    OriginalPrice = 0,
                    UploadedImageIds = [seed.VariantImageId],
                    AttributeValueIds = [seed.AttributeValueId],
                },
            ],
        };

        var exception = await Should.ThrowAsync<ValidationException>(() =>
            TestApp.SendAsync(command)
        );

        exception.Errors.ShouldContainKey("Variants[0].OriginalPrice");
    }

    [Test]
    public async Task ShouldReturnValidationForNullCollections()
    {
        await TestApp.RunAsDefaultUserAsync();
        var seed = await AddProductSeedDataAsync();
        var command = CreateValidCommand(seed) with
        {
            UploadedImageIds = null!,
            Variants =
            [
                new CreateProductVariantDto
                {
                    OriginalPrice = 120_000,
                    UploadedImageIds = null!,
                    AttributeValueIds = null!,
                },
            ],
        };

        var exception = await Should.ThrowAsync<ValidationException>(() =>
            TestApp.SendAsync(command)
        );

        exception.Errors.ShouldContainKey("UploadedImageIds");
        exception.Errors.ShouldContainKey("Variants[0].UploadedImageIds");
        exception.Errors.ShouldContainKey("Variants[0].AttributeValueIds");
    }

    [Test]
    public async Task ShouldRejectUploadedImagesOwnedByAnotherUser()
    {
        await TestApp.RunAsUserAsync("owner@local", "Testing1234!", []);
        var seed = await AddProductSeedDataAsync();

        await TestApp.RunAsUserAsync("other@local", "Testing1234!", []);
        var command = CreateValidCommand(seed);

        var exception = await Should.ThrowAsync<ValidationException>(() =>
            TestApp.SendAsync(command)
        );

        exception.Errors.ShouldContainKey("UploadedImageIds");
    }

    [Test]
    public async Task ShouldMarkUploadedFilesAsUsedAfterSuccessfulCreation()
    {
        await TestApp.RunAsDefaultUserAsync();
        var seed = await AddProductSeedDataAsync();

        await TestApp.SendAsync(CreateValidCommand(seed));

        var productImage = await TestApp.FindAsync<UploadedFile>(seed.ProductImageId);
        var variantImage = await TestApp.FindAsync<UploadedFile>(seed.VariantImageId);

        productImage.ShouldNotBeNull();
        productImage!.IsUsed.ShouldBeTrue();
        variantImage.ShouldNotBeNull();
        variantImage!.IsUsed.ShouldBeTrue();
    }

    [Test]
    public async Task EndpointShouldDenyAnonymousCreateProductRequests()
    {
        using var client = TestApp.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/products",
            new
            {
                Name = "Road Runner Sneaker",
                CategoryId = 1,
                BrandId = 1,
                UploadedImageIds = new[] { 1 },
                Variants = new[]
                {
                    new
                    {
                        OriginalPrice = 120_000,
                        UploadedImageIds = new[] { 2 },
                        AttributeValueIds = new[] { 1 },
                    },
                },
            }
        );

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private static CreateProductCommand CreateValidCommand(ProductSeedData seed)
    {
        return new CreateProductCommand
        {
            Name = "Road Runner Sneaker",
            Description = "Lightweight running shoe",
            CategoryId = seed.CategoryId,
            BrandId = seed.BrandId,
            UploadedImageIds = [seed.ProductImageId],
            Variants =
            [
                new CreateProductVariantDto
                {
                    OriginalPrice = 120_000,
                    UploadedImageIds = [seed.VariantImageId],
                    AttributeValueIds = [seed.AttributeValueId],
                },
            ],
        };
    }

    private static async Task<ProductSeedData> AddProductSeedDataAsync()
    {
        var category = new Category("Shoes");
        await TestApp.AddAsync(category);

        var brand = new Brand("Contoso");
        await TestApp.AddAsync(brand);

        var attribute = new ProductAttribute("Size");
        await TestApp.AddAsync(attribute);

        var attributeValue = new ProductAttributeValue(attribute.Id, "42");
        await TestApp.AddAsync(attributeValue);

        var productImage = CreateUploadedFile("product");
        await TestApp.AddAsync(productImage);

        var variantImage = CreateUploadedFile("variant");
        await TestApp.AddAsync(variantImage);

        return new ProductSeedData(
            category.Id,
            brand.Id,
            attributeValue.Id,
            productImage.Id,
            variantImage.Id
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

    private sealed record ProductSeedData(
        int CategoryId,
        int BrandId,
        int AttributeValueId,
        int ProductImageId,
        int VariantImageId
    );
}
