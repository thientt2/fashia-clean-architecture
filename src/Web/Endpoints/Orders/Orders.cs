using Fashia.Application.Orders.Commands.PlaceOrder;
using Fashia.Web.Endpoints.Orders.Requests;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Fashia.Web.Endpoints.Orders;

public class Orders : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        // groupBuilder.MapGet(GetOrders);
        // groupBuilder.MapGet(GetOrderById, "{id:int}");
        // groupBuilder.MapPost(CreateOrder);
        groupBuilder.MapPost(PlaceOrder, "placeorder").RequireAuthorization();
    }

    // [EndpointSummary("Get all Orders")]
    // [EndpointDescription("Retrieves all orders.")]
    // public static async Task<Ok<IReadOnlyCollection<OrderDto>>> GetOrders(ISender sender)
    // {
    //     var orders = await sender.Send(new GetOrdersQuery());

    //     return TypedResults.Ok(orders);
    // }

    // [EndpointSummary("Get Order by Id")]
    // [EndpointDescription("Retrieves an order by id.")]
    // public static async Task<Results<Ok<OrderDto>, NotFound>> GetOrderById(ISender sender, int id)
    // {
    //     var order = await sender.Send(new GetOrderByIdQuery(id));

    //     return order is null ? TypedResults.NotFound() : TypedResults.Ok(order);
    // }

    // [EndpointSummary("Create Order")]
    // [EndpointDescription(
    //     "Creates an order, decreases branch inventory, and records sale transactions."
    // )]
    // public static async Task<Created<int>> CreateOrder(
    //     ISender sender,
    //     CreateOrderRequest request,
    //     CancellationToken cancellationToken
    // )
    // {
    //     var id = await sender.Send(
    //         new CreateOrderCommand
    //         {
    //             CustomerId = request.CustomerId,
    //             BranchId = request.BranchId,
    //             VoucherCode = request.VoucherCode,
    //             Items = request
    //                 .Items.Select(x => new CreateOrderItemDto
    //                 {
    //                     ProductVariantId = x.ProductVariantId,
    //                     Quantity = x.Quantity,
    //                 })
    //                 .ToList(),
    //         },
    //         cancellationToken
    //     );

    //     return TypedResults.Created($"/api/orders/{id}", id);
    // }

    [EndpointSummary("Place order")]
    [EndpointDescription(
        "Creates a pending order from the authenticated customer's cart and reserves inventory at the nearest fulfillable branch."
    )]
    public static async Task<Created<OrderCreated>> PlaceOrder(
        ISender sender,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        PlaceOrderRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await sender.Send(
            new PlaceOrderCommand
            {
                IdempotencyKey = idempotencyKey ?? string.Empty,
                ShippingAddress = new PlaceOrderShippingAddressDto
                {
                    CustomerName = request.ShippingAddress.CustomerName,
                    CustomerEmail = request.ShippingAddress.CustomerEmail,
                    CustomerPhone = request.ShippingAddress.CustomerPhone,
                    Line1 = request.ShippingAddress.Line1,
                    Ward = request.ShippingAddress.Ward,
                    District = request.ShippingAddress.District,
                    Province = request.ShippingAddress.Province,
                    Latitude = request.ShippingAddress.Latitude,
                    Longitude = request.ShippingAddress.Longitude,
                },
                VoucherCode = request.VoucherCode,
                PaymentMethod = request.PaymentMethod,
                Note = request.Note,
            },
            cancellationToken
        );

        return TypedResults.Created($"/api/orders/{result.OrderId}", result);
    }
}
