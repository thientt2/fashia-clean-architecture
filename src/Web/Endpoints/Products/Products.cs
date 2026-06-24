using Fashia.Application.Common.Models;
using Fashia.Application.Products.Commands.CreateProduct;
using Fashia.Application.Products.Commands.DeleteProduct;
using Fashia.Application.Products.Commands.UpdateProduct;
using Fashia.Application.Products.Queries.Common;
using Fashia.Application.Products.Queries.GetProductById;
using Fashia.Application.Products.Queries.GetProducts;
using Fashia.Domain.Constants;
using Fashia.Web.Endpoints.Products.Requests;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Fashia.Web.Endpoints.Products;

public class Products : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetProducts);
        groupBuilder.MapGet(GetProductById, "{id:int}");

        groupBuilder.MapPost(CreateProduct).RequireAuthorization();

        groupBuilder.MapPatch(UpdateProduct, "{id:int}").RequireAuthorization();

        groupBuilder.MapDelete(DeleteProduct, "{id:int}").RequireAuthorization();
    }

    [EndpointSummary("Get all Products")]
    [EndpointDescription("Retrieves all products.")]
    public static async Task<Ok<PaginatedList<ProductListItemDto>>> GetProducts(
        [FromServices] ISender sender,
        [AsParameters] GetProductsQuery query,
        CancellationToken cancellationToken
    )
    {
        var products = await sender.Send(query, cancellationToken);

        return TypedResults.Ok(products);
    }

    [EndpointSummary("Get Product by Id")]
    [EndpointDescription("Retrieves a product by id.")]
    public static async Task<Results<Ok<ProductDetailDto>, NotFound>> GetProductById(
        [FromServices] ISender sender,
        [FromRoute] int id,
        CancellationToken cancellationToken
    )
    {
        var product = await sender.Send(new GetProductByIdQuery(id), cancellationToken);

        return product is null ? TypedResults.NotFound() : TypedResults.Ok(product);
    }

    [EndpointSummary("Create a new Product")]
    [EndpointDescription(
        "Creates a new product using the provided details and returns the ID of the created product."
    )]
    public static async Task<Created<int>> CreateProduct(
        [FromServices] ISender sender,
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken
    )
    {
        var command = new CreateProductCommand
        {
            Name = request.Name,
            Description = request.Description,
            UploadedImageIds = request.UploadedImageIds ?? [],
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            Variants =
                request
                    .Variants?.Select(x =>
                        x is null
                            ? new CreateProductVariantDto()
                            : new CreateProductVariantDto
                            {
                                OriginalPrice = x.OriginalPrice,
                                UploadedImageIds = x.UploadedImageIds ?? [],
                                AttributeValueIds = x.AttributeValueIds ?? [],
                            }
                    )
                    .ToList()
                ?? [],
        };

        var id = await sender.Send(command, cancellationToken);

        return TypedResults.Created($"/api/products/{id}", id);
    }

    [EndpointSummary("Update Product")]
    [EndpointDescription("Updates an existing product.")]
    public static async Task<Results<NoContent, NotFound>> UpdateProduct(
        [FromServices] ISender sender,
        [FromRoute] int id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken
    )
    {
        var command = new UpdateProductCommand
        {
            Id = id, // inject từ route
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            NewUploadedImageIds = request.NewUploadedImageIds,
        };

        await sender.Send(command, cancellationToken);

        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete Product")]
    [EndpointDescription("Deletes an existing product.")]
    public static async Task<NoContent> DeleteProduct(
        [FromServices] ISender sender,
        [FromRoute] int id
    )
    {
        await sender.Send(new DeleteProductCommand(id));

        return TypedResults.NoContent();
    }
}
