using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fashia.Application.Categories.Commands.CreateCategory;

public record CreateCategoryCommand : IRequest<int>
{
    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string? ImageUrl { get; init; }

    public int? ParentId { get; init; }
}

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (request.ParentId.HasValue)
        {
            var parentExists = await _context.Categories
                .AnyAsync(x => x.Id == request.ParentId.Value, cancellationToken);

            if (!parentExists)
                throw new InvalidOperationException("Parent category does not exist.");
        }

        var category = new Category(
            request.Name,
            request.Description,
            request.ImageUrl,
            request.ParentId);

        var slugExists = await _context.Categories
            .AnyAsync(x => x.Slug == category.Slug, cancellationToken);

        if (slugExists)
            throw new InvalidOperationException("Category slug already exists.");

        _context.Categories.Add(category);

        await _context.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}