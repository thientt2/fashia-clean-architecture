using Fashia.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fashia.Application.Categories.Commands.ActivateCategory;

public record ActivateCategoryCommand(int Id) : IRequest;

public class ActivateCategoryCommandHandler : IRequestHandler<ActivateCategoryCommand>
{
    private readonly IApplicationDbContext _context;

    public ActivateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ActivateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (category is null)
            throw new InvalidOperationException("Category not found.");

        category.Activate();

        await _context.SaveChangesAsync(cancellationToken);
    }
}