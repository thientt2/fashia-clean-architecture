using System.Text.Json.Serialization;
using Fashia.Application.Common.Interfaces;

namespace Fashia.Application.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand : IRequest
{
    [JsonIgnore]
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string? ImageUrl { get; init; }

    public int CategoryId { get; init; }

    public int BrandId { get; init; }
}

public sealed class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (product is null)
            throw new InvalidOperationException("Product not found.");

        var categoryExists = await _context.Categories
            .AnyAsync(x => x.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
            throw new InvalidOperationException("Category not found.");

        var brandExists = await _context.Brands
            .AnyAsync(x => x.Id == request.BrandId, cancellationToken);

        if (!brandExists)
            throw new InvalidOperationException("Brand not found.");

        product.Rename(request.Name);
        product.UpdateDescription(request.Description);
        product.UpdateImageUrl(request.ImageUrl);
        product.ChangeCategory(request.CategoryId);
        product.ChangeBrand(request.BrandId);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
