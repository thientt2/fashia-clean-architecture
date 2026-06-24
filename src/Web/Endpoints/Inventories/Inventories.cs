using Fashia.Application.Inventories.Commands.AdjustInventoryStock;
using Fashia.Application.Inventories.Commands.CommitReservedInventory;
using Fashia.Application.Inventories.Commands.DecreaseInventoryStock;
using Fashia.Application.Inventories.Commands.IncreaseInventoryStock;
using Fashia.Application.Inventories.Commands.InitializeInventory;
using Fashia.Application.Inventories.Commands.ReleaseReservedInventory;
using Fashia.Application.Inventories.Commands.ReserveInventoryStock;
using Fashia.Application.Inventories.Commands.ReturnInventoryStock;
using Fashia.Application.Inventories.Commands.TransferInventoryStock;
using Fashia.Application.Inventories.Queries.Common;
using Fashia.Application.Inventories.Queries.GetInventoryByBranch;
using Fashia.Application.Inventories.Queries.GetInventoryByProduct;
using Fashia.Application.Inventories.Queries.GetInventoryTransactionHistory;
using Fashia.Application.Inventories.Queries.GetLowStockInventory;
using Fashia.Domain.Constants;
using Fashia.Domain.Enums;
using Fashia.Web.Endpoints.Inventories.Requests;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Fashia.Web.Endpoints.Inventories;

public class Inventories : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetInventoryByBranch, "by-branch/{branchId:int}");
        groupBuilder.MapGet(GetInventoryByProduct, "by-product/{productId:int}");
        groupBuilder.MapGet(GetLowStockInventory, "low-stock");
        groupBuilder.MapGet(GetInventoryTransactionHistory, "transactions");

        groupBuilder
            .MapPost(InitializeInventory, "{branchId:int}/initialize")
            .RequireAuthorization();
        groupBuilder
            .MapPost(IncreaseInventoryStock, "{branchId:int}/increase")
            .RequireAuthorization();
        groupBuilder
            .MapPost(DecreaseInventoryStock, "{branchId:int}/decrease")
            .RequireAuthorization();
        groupBuilder.MapPost(AdjustInventoryStock, "{branchId:int}/adjust").RequireAuthorization();
        groupBuilder
            .MapPost(
                TransferInventoryStock,
                "{sourceBranchId:int}/transfer/{destinationBranchId:int}"
            )
            .RequireAuthorization();
        groupBuilder
            .MapPost(ReserveInventoryStock, "{branchId:int}/reserve")
            .RequireAuthorization();
        groupBuilder
            .MapPost(ReleaseReservedInventory, "{branchId:int}/release-reservation")
            .RequireAuthorization();
        groupBuilder
            .MapPost(CommitReservedInventory, "{branchId:int}/commit-reservation")
            .RequireAuthorization();
        groupBuilder.MapPost(ReturnInventoryStock, "{branchId:int}/return").RequireAuthorization();
    }

    [EndpointSummary("Get Inventory by Branch")]
    [EndpointDescription("Retrieves inventory rows for a branch.")]
    public static async Task<Ok<IReadOnlyCollection<InventoryDto>>> GetInventoryByBranch(
        [FromServices] ISender sender,
        [FromRoute] int branchId,
        CancellationToken cancellationToken
    )
    {
        var inventory = await sender.Send(
            new GetInventoryByBranchQuery(branchId),
            cancellationToken
        );

        return TypedResults.Ok(inventory);
    }

    [EndpointSummary("Get Inventory by Product")]
    [EndpointDescription("Retrieves inventory rows across branches for a product.")]
    public static async Task<Ok<IReadOnlyCollection<InventoryDto>>> GetInventoryByProduct(
        [FromServices] ISender sender,
        [FromRoute] int productId,
        CancellationToken cancellationToken
    )
    {
        var inventory = await sender.Send(
            new GetInventoryByProductQuery(productId),
            cancellationToken
        );

        return TypedResults.Ok(inventory);
    }

    [EndpointSummary("Get Low Stock Inventory")]
    [EndpointDescription(
        "Retrieves inventory rows with available stock at or below the v1 threshold."
    )]
    public static async Task<Ok<IReadOnlyCollection<InventoryDto>>> GetLowStockInventory(
        [FromServices] ISender sender,
        CancellationToken cancellationToken
    )
    {
        var inventory = await sender.Send(new GetLowStockInventoryQuery(), cancellationToken);

        return TypedResults.Ok(inventory);
    }

    [EndpointSummary("Get Inventory Transaction History")]
    [EndpointDescription("Retrieves paged inventory transactions with optional filters.")]
    public static async Task<Ok<InventoryTransactionHistoryResult>> GetInventoryTransactionHistory(
        [FromServices] ISender sender,
        [FromQuery] int? branchId,
        [FromQuery] int? productVariantId,
        [FromQuery] InventoryTransactionType? type,
        [FromQuery] int? orderId,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken
    )
    {
        var history = await sender.Send(
            new GetInventoryTransactionHistoryQuery
            {
                BranchId = branchId,
                ProductVariantId = productVariantId,
                Type = type,
                OrderId = orderId,
                From = from,
                To = to,
                PageNumber = pageNumber,
                PageSize = pageSize,
            },
            cancellationToken
        );

        return TypedResults.Ok(history);
    }

    [EndpointSummary("Initialize Inventory")]
    [EndpointDescription("Initializes inventory for a branch and product variant.")]
    public static async Task<NoContent> InitializeInventory(
        [FromServices] ISender sender,
        [FromRoute] int branchId,
        [FromBody] InventoryStockRequest request,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(
            new InitializeInventoryCommand
            {
                BranchId = branchId,
                ProductVariantId = request.ProductVariantId,
                Quantity = request.Quantity,
                Note = request.Note,
            },
            cancellationToken
        );

        return TypedResults.NoContent();
    }

    [EndpointSummary("Increase Inventory Stock")]
    [EndpointDescription("Increases stock for an existing branch inventory row.")]
    public static async Task<NoContent> IncreaseInventoryStock(
        [FromRoute] int branchId,
        [FromServices] ISender sender,
        [FromBody] InventoryStockRequest request,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(
            new IncreaseInventoryStockCommand
            {
                BranchId = branchId,
                ProductVariantId = request.ProductVariantId,
                Quantity = request.Quantity,
                Note = request.Note,
            },
            cancellationToken
        );

        return TypedResults.NoContent();
    }

    [EndpointSummary("Decrease Inventory Stock")]
    [EndpointDescription("Decreases available stock for an existing branch inventory row.")]
    public static async Task<NoContent> DecreaseInventoryStock(
        [FromRoute] int branchId,
        [FromServices] ISender sender,
        [FromBody] InventoryStockRequest request,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(
            new DecreaseInventoryStockCommand
            {
                BranchId = branchId,
                ProductVariantId = request.ProductVariantId,
                Quantity = request.Quantity,
                Note = request.Note,
            },
            cancellationToken
        );

        return TypedResults.NoContent();
    }

    [EndpointSummary("Adjust Inventory Stock")]
    [EndpointDescription("Sets on-hand stock for an existing branch inventory row.")]
    public static async Task<NoContent> AdjustInventoryStock(
        [FromRoute] int branchId,
        [FromServices] ISender sender,
        [FromBody] InventoryStockRequest request,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(
            new AdjustInventoryStockCommand
            {
                BranchId = branchId,
                ProductVariantId = request.ProductVariantId,
                Quantity = request.Quantity,
                Note = request.Note,
            },
            cancellationToken
        );

        return TypedResults.NoContent();
    }

    [EndpointSummary("Transfer Inventory Stock")]
    [EndpointDescription("Transfers available stock between branches.")]
    public static async Task<NoContent> TransferInventoryStock(
        [FromRoute] int sourceBranchId,
        [FromRoute] int destinationBranchId,
        [FromServices] ISender sender,
        [FromBody] TransferInventoryStockRequest request,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(
            new TransferInventoryStockCommand
            {
                SourceBranchId = sourceBranchId,
                DestinationBranchId = destinationBranchId,
                ProductVariantId = request.ProductVariantId,
                Quantity = request.Quantity,
                Note = request.Note,
            },
            cancellationToken
        );

        return TypedResults.NoContent();
    }

    [EndpointSummary("Reserve Inventory Stock")]
    [EndpointDescription("Reserves available stock for an order.")]
    public static async Task<NoContent> ReserveInventoryStock(
        [FromRoute] int branchId,
        [FromServices] ISender sender,
        [FromBody] InventoryReservationRequest request,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(
            new ReserveInventoryStockCommand
            {
                BranchId = branchId,
                ProductVariantId = request.ProductVariantId,
                OrderId = request.OrderId,
                Quantity = request.Quantity,
                Note = request.Note,
            },
            cancellationToken
        );

        return TypedResults.NoContent();
    }

    [EndpointSummary("Release Reserved Inventory")]
    [EndpointDescription("Releases reserved stock for an order.")]
    public static async Task<NoContent> ReleaseReservedInventory(
        [FromRoute] int branchId,
        [FromServices] ISender sender,
        [FromBody] InventoryReservationRequest request,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(
            new ReleaseReservedInventoryCommand
            {
                BranchId = branchId,
                ProductVariantId = request.ProductVariantId,
                OrderId = request.OrderId,
                Quantity = request.Quantity,
                Note = request.Note,
            },
            cancellationToken
        );

        return TypedResults.NoContent();
    }

    [EndpointSummary("Commit Reserved Inventory")]
    [EndpointDescription("Commits reserved stock for an order.")]
    public static async Task<NoContent> CommitReservedInventory(
        [FromRoute] int branchId,
        [FromServices] ISender sender,
        [FromBody] InventoryReservationRequest request,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(
            new CommitReservedInventoryCommand
            {
                BranchId = branchId,
                ProductVariantId = request.ProductVariantId,
                OrderId = request.OrderId,
                Quantity = request.Quantity,
                Note = request.Note,
            },
            cancellationToken
        );

        return TypedResults.NoContent();
    }

    [EndpointSummary("Return Inventory Stock")]
    [EndpointDescription("Returns stock to branch inventory.")]
    public static async Task<NoContent> ReturnInventoryStock(
        [FromRoute] int branchId,
        [FromServices] ISender sender,
        [FromBody] ReturnInventoryStockRequest request,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(
            new ReturnInventoryStockCommand
            {
                BranchId = branchId,
                ProductVariantId = request.ProductVariantId,
                OrderId = request.OrderId,
                Quantity = request.Quantity,
                Note = request.Note,
            },
            cancellationToken
        );

        return TypedResults.NoContent();
    }
}
