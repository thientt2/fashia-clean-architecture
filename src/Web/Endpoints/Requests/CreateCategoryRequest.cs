namespace Fashia.Web.Endpoints.Requests;

public class CreateCategoryRequest
{
    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public int? ParentId { get; init; }

    public IFormFile? Image { get; init; }
}