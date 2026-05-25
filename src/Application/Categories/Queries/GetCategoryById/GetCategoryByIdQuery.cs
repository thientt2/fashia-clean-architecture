using Fashia.Application.Categories.Queries.GetCategories;
using Fashia.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fashia.Application.Categories.Queries.GetCategoryById;

public record GetCategoryByIdQuery(int Id) : IRequest<CategoryDto?>;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto?>
{
    private readonly IApplicationDbContext _context;

    public GetCategoryByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto?> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new CategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Slug = x.Slug,
                ImageUrl = x.ImageUrl,
                ParentId = x.ParentId
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}