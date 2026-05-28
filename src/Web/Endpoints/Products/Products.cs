using System.Text.Json;
using Fashia.Application.Common.Interfaces;
using Fashia.Application.Common.Models;
using Fashia.Application.Products.Commands.CreateProduct;
using Fashia.Application.Products.Commands.DeleteProduct;
using Fashia.Application.Products.Commands.UpdateProduct;
using Fashia.Application.Products.Queries;
using Fashia.Domain.Constants;
using Fashia.Web.Endpoints.Requests;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Fashia.Web.Endpoints;

public class Products : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetProducts);
        groupBuilder.MapGet(GetProductById, "{id:int}");

        groupBuilder
            .MapPost(CreateProduct)
            .DisableAntiforgery()
            .RequireAuthorization(Policies.CanManageProducts);

        groupBuilder
            .MapPut(UpdateProduct, "{id:int}")
            .RequireAuthorization(Policies.CanManageProducts);

        groupBuilder
            .MapDelete(DeleteProduct, "{id:int}")
            .RequireAuthorization(Policies.CanManageProducts);
    }

    [EndpointSummary("Get all Products")]
    [EndpointDescription("Retrieves all products.")]
    public static async Task<Ok<IReadOnlyCollection<ProductDto>>> GetProducts(ISender sender)
    {
        var products = await sender.Send(new GetProductsQuery());

        return TypedResults.Ok(products);
    }

    [EndpointSummary("Get Product by Id")]
    [EndpointDescription("Retrieves a product by id.")]
    public static async Task<Results<Ok<ProductDto>, NotFound>> GetProductById(
        ISender sender,
        int id
    )
    {
        var product = await sender.Send(new GetProductByIdQuery(id));

        return product is null ? TypedResults.NotFound() : TypedResults.Ok(product);
    }

    [EndpointSummary("Create a new Product")]
    [EndpointDescription(
        "Creates a new product using the provided details and returns the ID of the created product."
    )]
    public static async Task<Results<Created<int>, BadRequest<string>>> CreateProduct(
        ISender sender,
        [FromForm] CreateProductRequest request,
        CancellationToken cancellationToken
    )
    {
        List<CreateProductVariantRequest> variantRequests;

        try
        {
            variantRequests =
                JsonSerializer.Deserialize<List<CreateProductVariantRequest>>(
                    request.Variants,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? [];
        }
        catch (JsonException)
        {
            return TypedResults.BadRequest("Invalid variants JSON format.");
        }

        if (variantRequests.Count == 0)
        {
            return TypedResults.BadRequest("At least one product variant is required.");
        }

        var id = await sender.Send(
            new CreateProductCommand
            {
                Name = request.Name,
                Description = request.Description,
                Image = request.Image is not null
                    ? new FileUpload(
                        request.Image.OpenReadStream(),
                        request.Image.FileName,
                        request.Image.ContentType
                    )
                    : null,
                CategoryId = request.CategoryId,
                BrandId = request.BrandId,
                Variants = variantRequests
                    .Select(x => new CreateProductVariantDto
                    {
                        OriginalPrice = x.OriginalPrice,
                        StockQuantity = x.StockQuantity,
                        AttributeValueIds = x.AttributeValueIds,
                    })
                    .ToList(),
            },
            cancellationToken
        );

        return TypedResults.Created($"/api/products/{id}", id);
    }

    [EndpointSummary("Update Product")]
    [EndpointDescription("Updates an existing product.")]
    public static async Task<Results<NoContent, BadRequest>> UpdateProduct(
        ISender sender,
        int id,
        UpdateProductCommand command
    )
    {
        if (command.Id != 0 && id != command.Id)
            return TypedResults.BadRequest();

        await sender.Send(command with { Id = id });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete Product")]
    [EndpointDescription("Deletes an existing product.")]
    public static async Task<NoContent> DeleteProduct(ISender sender, int id)
    {
        await sender.Send(new DeleteProductCommand(id));

        return TypedResults.NoContent();
    }
}
