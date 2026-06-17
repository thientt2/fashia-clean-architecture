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
            .MapPost(InitializeInventory, "initialize")
            .RequireAuthorization(Policies.CanManageBranchInventories);
        groupBuilder
            .MapPost(IncreaseInventoryStock, "increase")
            .RequireAuthorization(Policies.CanManageBranchInventories);
        groupBuilder
            .MapPost(DecreaseInventoryStock, "decrease")
            .RequireAuthorization(Policies.CanManageBranchInventories);
        groupBuilder
            .MapPost(AdjustInventoryStock, "adjust")
            .RequireAuthorization(Policies.CanManageBranchInventories);
        groupBuilder
            .MapPost(TransferInventoryStock, "transfer")
            .RequireAuthorization(Policies.CanManageBranchInventories);
        groupBuilder
            .MapPost(ReserveInventoryStock, "reserve")
            .RequireAuthorization(Policies.CanManageBranchInventories);
        groupBuilder
            .MapPost(ReleaseReservedInventory, "release-reservation")
            .RequireAuthorization(Policies.CanManageBranchInventories);
        groupBuilder
            .MapPost(CommitReservedInventory, "commit-reservation")
            .RequireAuthorization(Policies.CanManageBranchInventories);
        groupBuilder
            .MapPost(ReturnInventoryStock, "return")
            .RequireAuthorization(Policies.CanManageBranchInventories);
    }

    [EndpointSummary("Get Inventory by Branch")]
    [EndpointDescription("Retrieves inventory rows for a branch.")]
    public static async Task<Ok<IReadOnlyCollection<InventoryDto>>> GetInventoryByBranch(
        ISender sender,
        int branchId,
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
        ISender sender,
        int productId,
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
    [EndpointDescription("Retrieves inventory rows with available stock at or below the v1 threshold.")]
    public static async Task<Ok<IReadOnlyCollection<InventoryDto>>> GetLowStockInventory(
        ISender sender,
        CancellationToken cancellationToken
    )
    {
        var inventory = await sender.Send(new GetLowStockInventoryQuery(), cancellationToken);

        return TypedResults.Ok(inventory);
    }

    [EndpointSummary("Get Inventory Transaction History")]
    [EndpointDescription("Retrieves paged inventory transactions with optional filters.")]
    public static async Task<Ok<InventoryTransactionHistoryResult>> GetInventoryTransactionHistory(
        ISender sender,
        int? branchId,
        int? productVariantId,
        InventoryTransactionType? type,
        int? orderId,
        DateTimeOffset? from,
        DateTimeOffset? to,
        int pageNumber,
        int pageSize,
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
        ISender sender,
        InventoryStockRequest request,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(
            new InitializeInventoryCommand
            {
                BranchId = request.BranchId,
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
        ISender sender,
        InventoryStockRequest request,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(
            new IncreaseInventoryStockCommand
            {
                BranchId = request.BranchId,
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
        ISender sender,
        InventoryStockRequest request,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(
            new DecreaseInventoryStockCommand
            {
                BranchId = request.BranchId,
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
        ISender sender,
        InventoryStockRequest request,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(
            new AdjustInventoryStockCommand
            {
                BranchId = request.BranchId,
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
        ISender sender,
        TransferInventoryStockRequest request,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(
            new TransferInventoryStockCommand
            {
                SourceBranchId = request.SourceBranchId,
                DestinationBranchId = request.DestinationBranchId,
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
        ISender sender,
        InventoryReservationRequest request,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(
            new ReserveInventoryStockCommand
            {
                BranchId = request.BranchId,
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
        ISender sender,
        InventoryReservationRequest request,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(
            new ReleaseReservedInventoryCommand
            {
                BranchId = request.BranchId,
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
        ISender sender,
        InventoryReservationRequest request,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(
            new CommitReservedInventoryCommand
            {
                BranchId = request.BranchId,
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
        ISender sender,
        ReturnInventoryStockRequest request,
        CancellationToken cancellationToken
    )
    {
        await sender.Send(
            new ReturnInventoryStockCommand
            {
                BranchId = request.BranchId,
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
