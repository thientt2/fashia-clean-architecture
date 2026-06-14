using Fashia.Application.Carts.Commands.AddCartItem;
using Fashia.Application.Carts.Commands.ClearCart;
using Fashia.Application.Carts.Commands.RemoveCartItem;
using Fashia.Application.Carts.Commands.UpdateCartItemQuantity;
using Fashia.Application.Carts.Queries.GetCurrentCart;
using Fashia.Web.Endpoints.Requests;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fashia.Web.Endpoints;

public class Cart : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();
        groupBuilder.MapGet(GetCurrentCart, "current");
        groupBuilder.MapPost(AddCartItem, "items");
        groupBuilder.MapPut(UpdateCartItemQuantity, "items/{cartItemId:int}");
        groupBuilder.MapDelete(RemoveCartItem, "items/{cartItemId:int}");
        groupBuilder.MapDelete(ClearCart, "items");
    }

    [EndpointSummary("Get current Cart")]
    [EndpointDescription("Retrieves the active cart for the current user.")]
    public static async Task<Ok<CartDto>> GetCurrentCart(
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        var cart = await sender.Send(new GetCurrentCartQuery(), cancellationToken);

        return TypedResults.Ok(cart);
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
    [EndpointDescription("Updates quantity for a cart item in the active cart.")]
    public static async Task<Ok<CartDto>> UpdateCartItemQuantity(
        ISender sender,
        int cartItemId,
        UpdateCartItemQuantityRequest request,
        CancellationToken cancellationToken
    )
    {
        var cart = await sender.Send(
            new UpdateCartItemQuantityCommand
            {
                CartItemId = cartItemId,
                Quantity = request.Quantity,
            },
            cancellationToken
        );

        return TypedResults.Ok(cart);
    }

    [EndpointSummary("Remove Cart item")]
    [EndpointDescription("Removes a cart item from the active cart.")]
    public static async Task<Ok<CartDto>> RemoveCartItem(
        ISender sender,
        int cartItemId,
        CancellationToken cancellationToken
    )
    {
        var cart = await sender.Send(
            new RemoveCartItemCommand { CartItemId = cartItemId },
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
