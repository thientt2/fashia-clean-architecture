using Fashia.Application.Carts.Commands.AddCartItem;
using Fashia.Application.Carts.Commands.ClearCart;
using Fashia.Application.Carts.Commands.RemoveCartItem;
using Fashia.Application.Carts.Commands.UpdateCartItemQuantity;
using Fashia.Application.Carts.Queries;
using Fashia.Application.Carts.Queries.GetCurrentCart;
using Fashia.Web.Endpoints.Requests;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fashia.Web.Endpoints;

public class Cart : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();
        groupBuilder.MapGet(GetCurrentCart);
        groupBuilder.MapPost(AddCartItem, "items");
        groupBuilder.MapPut(UpdateCartItemQuantity, "items/{productVariantId:int}");
        groupBuilder.MapDelete(RemoveCartItem, "items/{productVariantId:int}");
        groupBuilder.MapDelete(ClearCart, "");
    }

    [EndpointSummary("Get current Cart")]
    [EndpointDescription("Retrieves the active cart for the current user.")]
    public static async Task<Results<Ok<CartDto>, NotFound>> GetCurrentCart(
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        var cart = await sender.Send(new GetCurrentCartQuery(), cancellationToken);

        return cart is null ? TypedResults.NotFound() : TypedResults.Ok(cart);
    }

    [EndpointSummary("Add Cart item")]
    [EndpointDescription("Adds a product variant to the active cart.")]
    public static async Task<Ok<CartDto>> AddCartItem(
        ISender sender,
        AddCartItemRequest request,
        CancellationToken cancellationToken
    )
    {
        var cart = await sender.Send(
            new AddCartItemCommand
            {
                ProductVariantId = request.ProductVariantId,
                Quantity = request.Quantity,
            },
            cancellationToken
        );

        return TypedResults.Ok(cart);
    }

    [EndpointSummary("Update Cart item")]
    [EndpointDescription("Updates quantity for a product variant in the active cart.")]
    public static async Task<Ok<CartDto>> UpdateCartItemQuantity(
        ISender sender,
        int productVariantId,
        UpdateCartItemQuantityRequest request,
        CancellationToken cancellationToken
    )
    {
        var cart = await sender.Send(
            new UpdateCartItemQuantityCommand
            {
                ProductVariantId = productVariantId,
                Quantity = request.Quantity,
            },
            cancellationToken
        );

        return TypedResults.Ok(cart);
    }

    [EndpointSummary("Remove Cart item")]
    [EndpointDescription("Removes a product variant from the active cart.")]
    public static async Task<Ok<CartDto>> RemoveCartItem(
        ISender sender,
        int productVariantId,
        CancellationToken cancellationToken
    )
    {
        var cart = await sender.Send(
            new RemoveCartItemCommand
            {
                ProductVariantId = productVariantId,
            },
            cancellationToken
        );

        return TypedResults.Ok(cart);
    }

    [EndpointSummary("Clear Cart")]
    [EndpointDescription("Removes all items from the active cart.")]
    public static async Task<NoContent> ClearCart(
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(new ClearCartCommand(), cancellationToken);

        return TypedResults.NoContent();
    }
}
