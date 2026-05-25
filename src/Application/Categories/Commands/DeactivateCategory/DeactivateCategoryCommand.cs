using Fashia.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fashia.Application.Categories.Commands.DeactivateCategory;

public record DeactivateCategoryCommand(int Id) : IRequest;

public class DeactivateCategoryCommandHandler : IRequestHandler<DeactivateCategoryCommand>
{
    private readonly IApplicationDbContext _context;

    public DeactivateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeactivateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (category is null)
            throw new InvalidOperationException("Category not found.");

        category.Deactivate();

        await _context.SaveChangesAsync(cancellationToken);
    }
}