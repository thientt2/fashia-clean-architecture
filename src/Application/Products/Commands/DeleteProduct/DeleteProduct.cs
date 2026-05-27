using Fashia.Application.Common.Interfaces;

namespace Fashia.Application.Products.Commands.DeleteProduct;

public sealed record DeleteProductCommand(int Id) : IRequest;

public sealed class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (product is null)
            throw new InvalidOperationException("Product not found.");

        _context.Products.Remove(product);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
