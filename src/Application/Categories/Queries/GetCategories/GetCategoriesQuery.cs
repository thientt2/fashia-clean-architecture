using Fashia.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fashia.Application.Categories.Queries.GetCategories;

public record GetCategoriesQuery : IRequest<IReadOnlyCollection<CategoryDto>>;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, IReadOnlyCollection<CategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<CategoryDto>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new CategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Slug = x.Slug,
                ImageUrl = x.ImageUrl,
                ParentId = x.ParentId,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}