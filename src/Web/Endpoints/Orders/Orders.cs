using Fashia.Application.Orders.Commands.CreateOrder;
using Fashia.Application.Orders.Commands.CheckoutCart;
using Fashia.Application.Orders.Queries;
using Fashia.Web.Endpoints.Requests;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fashia.Web.Endpoints;

public class Orders : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetOrders);
        groupBuilder.MapGet(GetOrderById, "{id:int}");
        groupBuilder.MapPost(CreateOrder);
        groupBuilder.MapPost(CheckoutOrder, "checkout").RequireAuthorization();
    }

    [EndpointSummary("Get all Orders")]
    [EndpointDescription("Retrieves all orders.")]
    public static async Task<Ok<IReadOnlyCollection<OrderDto>>> GetOrders(ISender sender)
    {
        var orders = await sender.Send(new GetOrdersQuery());

        return TypedResults.Ok(orders);
    }

    [EndpointSummary("Get Order by Id")]
    [EndpointDescription("Retrieves an order by id.")]
    public static async Task<Results<Ok<OrderDto>, NotFound>> GetOrderById(
        ISender sender,
        int id
    )
    {
        var order = await sender.Send(new GetOrderByIdQuery(id));

        return order is null ? TypedResults.NotFound() : TypedResults.Ok(order);
    }

    [EndpointSummary("Create Order")]
    [EndpointDescription("Creates an order, decreases branch inventory, and records sale transactions.")]
    public static async Task<Created<int>> CreateOrder(
        ISender sender,
        CreateOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        var id = await sender.Send(
            new CreateOrderCommand
            {
                CustomerId = request.CustomerId,
                BranchId = request.BranchId,
                VoucherCode = request.VoucherCode,
                Items = request
                    .Items.Select(x => new CreateOrderItemDto
                    {
                        ProductVariantId = x.ProductVariantId,
                        Quantity = x.Quantity,
                    })
                    .ToList(),
            },
            cancellationToken
        );

        return TypedResults.Created($"/api/orders/{id}", id);
    }

    [EndpointSummary("Checkout Cart")]
    [EndpointDescription("Creates an order from the active cart for the current user.")]
    public static async Task<Created<int>> CheckoutOrder(
        ISender sender,
        CheckoutOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        var id = await sender.Send(
            new CheckoutCartCommand
            {
                BranchId = request.BranchId,
                VoucherCode = request.VoucherCode,
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                CustomerPhone = request.CustomerPhone,
                ShippingAddress = request.ShippingAddress,
            },
            cancellationToken
        );

        return TypedResults.Created($"/api/orders/{id}", id);
    }
}
