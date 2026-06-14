using Fashia.Application.Products.Commands.CreateProduct;
using Fashia.Application.Products.Commands.DeleteProduct;
using Fashia.Application.Products.Commands.UpdateProduct;
using Fashia.Application.Products.Queries.Common;
using Fashia.Application.Products.Queries.GetProductByIdQuery;
using Fashia.Application.Products.Queries.GetProductsQuery;
using Fashia.Domain.Constants;
using Fashia.Web.Endpoints.Products.Requests;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fashia.Web.Endpoints.Products;

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
            .MapPatch(UpdateProduct, "{id:int}")
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
    public static async Task<Created<int>> CreateProduct(
        ISender sender,
        CreateProductRequest request,
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
            Variants = request
                .Variants?
                .Select(x =>
                    x is null
                        ? new CreateProductVariantDto()
                        : new CreateProductVariantDto
                        {
                            OriginalPrice = x.OriginalPrice,
                            UploadedImageIds = x.UploadedImageIds ?? [],
                            AttributeValueIds = x.AttributeValueIds ?? [],
                        }
                )
                .ToList() ?? [],
        };

        var id = await sender.Send(command, cancellationToken);

        return TypedResults.Created($"/api/products/{id}", id);
    }

    [EndpointSummary("Update Product")]
    [EndpointDescription("Updates an existing product.")]
    public static async Task<Results<NoContent, NotFound>> UpdateProduct(
        ISender sender,
        int id,
        UpdateProductRequest request,
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
    public static async Task<NoContent> DeleteProduct(ISender sender, int id)
    {
        await sender.Send(new DeleteProductCommand(id));

        return TypedResults.NoContent();
    }
}
