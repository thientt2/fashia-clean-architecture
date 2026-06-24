using Fashia.Application.Categories.Commands.ActivateCategory;
using Fashia.Application.Categories.Commands.CreateCategory;
using Fashia.Application.Categories.Commands.DeactivateCategory;
using Fashia.Application.Categories.Commands.DeleteCategory;
using Fashia.Application.Categories.Commands.UpdateCategory;
using Fashia.Application.Categories.Queries.GetCategories;
using Fashia.Application.Categories.Queries.GetCategoryById;
using Fashia.Application.Common.Interfaces;
using Fashia.Web.Endpoints.Categories.Requests;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Fashia.Web.Endpoints.Categories;

public class Categories : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetCategories);
        groupBuilder.MapGet(GetCategoryById, "{id:int}");

        groupBuilder.MapPost(CreateCategory).RequireAuthorization();

        groupBuilder.MapPut(UpdateCategory, "{id:int}").RequireAuthorization();

        groupBuilder.MapDelete(DeleteCategory, "{id:int}").RequireAuthorization();

        groupBuilder.MapPatch("/activate/{id:int}", ActivateCategory).RequireAuthorization();

        groupBuilder.MapPatch("/deactivate/{id:int}", DeactivateCategory).RequireAuthorization();
    }

    [EndpointSummary("Get all Categories")]
    [EndpointDescription("Retrieves all categories.")]
    public static async Task<Ok<IReadOnlyCollection<CategoryDto>>> GetCategories(
        [FromServices] ISender sender
    )
    {
        var categories = await sender.Send(new GetCategoriesQuery());

        return TypedResults.Ok(categories);
    }

    [EndpointSummary("Get Category by Id")]
    [EndpointDescription("Retrieves a category by id.")]
    public static async Task<Results<Ok<CategoryDto>, NotFound>> GetCategoryById(
        [FromServices] ISender sender,
        [FromRoute] int id
    )
    {
        var category = await sender.Send(new GetCategoryByIdQuery(id));

        return category is null ? TypedResults.NotFound() : TypedResults.Ok(category);
    }

    [EndpointSummary("Create Category")]
    [EndpointDescription("Creates a new category.")]
    public static async Task<Created<int>> CreateCategory(
        [FromServices] ISender sender,
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken
    )
    {
        var id = await sender.Send(
            new CreateCategoryCommand
            {
                Name = request.Name,
                Description = request.Description,
                ImageUrl = request.ImageUrl,
                ParentId = request.ParentId,
            },
            cancellationToken
        );

        return TypedResults.Created($"/api/categories/{id}", id);
    }

    [EndpointSummary("Update Category")]
    [EndpointDescription("Updates an existing category.")]
    public static async Task<NoContent> UpdateCategory(
        [FromServices] ISender sender,
        [FromRoute] int id,
        [FromBody] UpdateCategoryCommand command
    )
    {
        if (id != command.Id)
            throw new InvalidOperationException("Route id does not match command id.");

        await sender.Send(command with { Id = id });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete Category")]
    [EndpointDescription("Deletes an existing category.")]
    public static async Task<NoContent> DeleteCategory(
        [FromServices] ISender sender,
        [FromRoute] int id
    )
    {
        await sender.Send(new DeleteCategoryCommand(id));

        return TypedResults.NoContent();
    }

    [EndpointSummary("Activate Category")]
    [EndpointDescription("Activates a category.")]
    public static async Task<NoContent> ActivateCategory(
        [FromServices] ISender sender,
        [FromRoute] int id
    )
    {
        await sender.Send(new ActivateCategoryCommand(id));

        return TypedResults.NoContent();
    }

    [EndpointSummary("Deactivate Category")]
    [EndpointDescription("Deactivates a category.")]
    public static async Task<NoContent> DeactivateCategory(
        [FromServices] ISender sender,
        [FromRoute] int id
    )
    {
        await sender.Send(new DeactivateCategoryCommand(id));

        return TypedResults.NoContent();
    }
}
