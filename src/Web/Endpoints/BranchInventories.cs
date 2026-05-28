using Fashia.Application.BranchInventories.Commands.ImportBranchInventory;
using Fashia.Application.Categories.Commands.ActivateCategory;
using Fashia.Application.Categories.Commands.CreateCategory;
using Fashia.Application.Categories.Commands.DeactivateCategory;
using Fashia.Application.Categories.Commands.DeleteCategory;
using Fashia.Application.Categories.Commands.UpdateCategory;
using Fashia.Application.Categories.Queries.GetCategories;
using Fashia.Application.Categories.Queries.GetCategoryById;
using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Constants;
using Fashia.Web.Endpoints.Requests;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Fashia.Web.Endpoints;

public class BranchInventories : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        // groupBuilder.RequireAuthorization();

        groupBuilder
            .MapPost(ImportBranchInventory)
            .RequireAuthorization(Policies.CanManageBranchInventories);
    }

    [EndpointSummary("Import Branch Inventory")]
    [EndpointDescription("Imports branch inventory.")]
    public static async Task<NoContent> ImportBranchInventory(
        ISender sender,
        ImportBranchInventoryCommand command
    )
    {
        await sender.Send(command);
        return TypedResults.NoContent();
    }
}
