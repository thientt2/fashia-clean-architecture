namespace Fashia.Web.Endpoints.Categories.Requests;

public class CreateCategoryRequest
{
    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public int? ParentId { get; init; }

    public string? ImageUrl { get; init; }
}
