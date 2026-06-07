using Fashia.Application.Orders.Commands.CreateOrder;
using Fashia.Application.Orders.Queries;
using Fashia.Web.Endpoints.Orders.Requests;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fashia.Web.Endpoints.Orders;

public class Orders : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetOrders);
        groupBuilder.MapGet(GetOrderById, "{id:int}");
        groupBuilder.MapPost(CreateOrder);
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
    public static async Task<Results<Ok<OrderDto>, NotFound>> GetOrderById(ISender sender, int id)
    {
        var order = await sender.Send(new GetOrderByIdQuery(id));

        return order is null ? TypedResults.NotFound() : TypedResults.Ok(order);
    }

    [EndpointSummary("Create Order")]
    [EndpointDescription(
        "Creates an order, decreases branch inventory, and records sale transactions."
    )]
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
}
