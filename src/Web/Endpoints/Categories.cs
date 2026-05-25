using Fashia.Application.Categories.Commands.ActivateCategory;
using Fashia.Application.Categories.Commands.CreateCategory;
using Fashia.Application.Categories.Commands.DeleteCategory;
using Fashia.Application.Categories.Commands.DeactivateCategory;
using Fashia.Application.Categories.Commands.UpdateCategory;
using Fashia.Application.Categories.Queries.GetCategories;
using Fashia.Application.Categories.Queries.GetCategoryById;
using Fashia.Domain.Constants;
using Microsoft.AspNetCore.Http.HttpResults;
using Fashia.Application.Common.Interfaces;
using Fashia.Web.Endpoints.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Fashia.Web.Endpoints;

public class Categories : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        // groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetCategories);
        groupBuilder.MapGet(GetCategoryById, "{id:int}");

        groupBuilder.MapPost(CreateCategory)
            .DisableAntiforgery()
            .RequireAuthorization(Policies.CanManageCategories);

        groupBuilder.MapPut(UpdateCategory, "{id:int}")
            .RequireAuthorization(Policies.CanManageCategories);

        groupBuilder.MapDelete(DeleteCategory, "{id:int}")
            .RequireAuthorization(Policies.CanManageCategories);

        groupBuilder.MapPatch("/activate/{id:int}", ActivateCategory)
            .RequireAuthorization(Policies.CanManageCategories);

        groupBuilder.MapPatch("/deactivate/{id:int}", DeactivateCategory)
            .RequireAuthorization(Policies.CanManageCategories);
    }

    [EndpointSummary("Get all Categories")]
    [EndpointDescription("Retrieves all categories.")]
    public static async Task<Ok<IReadOnlyCollection<CategoryDto>>> GetCategories(ISender sender)
    {
        var categories = await sender.Send(new GetCategoriesQuery());

        return TypedResults.Ok(categories);
    }

    [EndpointSummary("Get Category by Id")]
    [EndpointDescription("Retrieves a category by id.")]
    public static async Task<Results<Ok<CategoryDto>, NotFound>> GetCategoryById(
        ISender sender,
        int id)
    {
        var category = await sender.Send(new GetCategoryByIdQuery(id));

        return category is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(category);
    }

    [EndpointSummary("Create Category")]
    [EndpointDescription("Creates a new category.")]
    public static async Task<Created<int>> CreateCategory(
        ISender sender,
        IFileStorageService fileStorageService,
        [FromForm] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        string? imageUrl = null;

        if (request.Image is not null)
        {
            imageUrl = await fileStorageService.UploadAsync(
                request.Image.OpenReadStream(),
                request.Image.FileName,
                request.Image.ContentType,
                "categories",
                cancellationToken);
        }

        var id = await sender.Send(new CreateCategoryCommand
        {
            Name = request.Name,
            Description = request.Description,
            ImageUrl = imageUrl,
            ParentId = request.ParentId
        }, cancellationToken);


        return TypedResults.Created($"/api/categories/{id}", id);
    }

    [EndpointSummary("Update Category")]
    [EndpointDescription("Updates an existing category.")]
    public static async Task<NoContent> UpdateCategory(
        ISender sender,
        int id,
        UpdateCategoryCommand command)
    {
        if (id != command.Id)
            throw new InvalidOperationException("Route id does not match command id.");

        await sender.Send(command with { Id = id });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete Category")]
    [EndpointDescription("Deletes an existing category.")]
    public static async Task<NoContent> DeleteCategory(
        ISender sender,
        int id)
    {
        await sender.Send(new DeleteCategoryCommand(id));

        return TypedResults.NoContent();
    }

    [EndpointSummary("Activate Category")]
    [EndpointDescription("Activates a category.")]
    public static async Task<NoContent> ActivateCategory(
        ISender sender,
        int id)
    {
        await sender.Send(new ActivateCategoryCommand(id));

        return TypedResults.NoContent();
    }

    [EndpointSummary("Deactivate Category")]
    [EndpointDescription("Deactivates a category.")]
    public static async Task<NoContent> DeactivateCategory(
        ISender sender,
        int id)
    {
        await sender.Send(new DeactivateCategoryCommand(id));

        return TypedResults.NoContent();
    }
}