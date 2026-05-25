using System.Text.Json.Serialization;
using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fashia.Application.Categories.Commands.UpdateCategory;

public record UpdateCategoryCommand : IRequest
{
    [JsonIgnore]
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string? ImageUrl { get; init; }

    public int? ParentId { get; init; }
}

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (category is null)
            throw new InvalidOperationException("Category not found.");

        if (request.ParentId.HasValue)
        {
            if (request.ParentId.Value == request.Id)
                throw new InvalidOperationException("Category cannot be parent of itself.");

            var parentExists = await _context.Categories
                .AnyAsync(x => x.Id == request.ParentId.Value, cancellationToken);

            if (!parentExists)
                throw new InvalidOperationException("Parent category does not exist.");
        }

        var newSlug = SlugGenerator.Generate(request.Name);

        var slugExists = await _context.Categories
            .AnyAsync(x => x.Id != request.Id && x.Slug == newSlug, cancellationToken);

        if (slugExists)
            throw new InvalidOperationException("Category slug already exists.");

        category.Rename(request.Name);
        category.UpdateDescription(request.Description);
        category.UpdateImageUrl(request.ImageUrl);
        category.MoveToParent(request.ParentId);

        await _context.SaveChangesAsync(cancellationToken);
    }
}