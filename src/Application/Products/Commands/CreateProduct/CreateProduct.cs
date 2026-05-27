using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Entities;
using MediatR;

namespace Fashia.Application.Products.Commands.CreateProduct;

public sealed record CreateProductCommand : IRequest<int>
{
    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public string? ImageUrl { get; init; }

    public int CategoryId { get; init; }

    public int BrandId { get; init; }

    public List<CreateProductVariantDto> Variants { get; init; } = [];
}

public sealed record CreateProductVariantDto
{
    public decimal OriginalPrice { get; init; }

    public int StockQuantity { get; init; }

    public List<int> AttributeValueIds { get; init; } = [];
}

public sealed class CreateProductCommandHandler
    : IRequestHandler<CreateProductCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
        
    {
        var categoryExists = await _context.Categories
            .AnyAsync(x => x.Id == request.CategoryId, cancellationToken);

        if (!categoryExists)
            throw new InvalidOperationException("Category not found.");

        var brandExists = await _context.Brands
            .AnyAsync(x => x.Id == request.BrandId, cancellationToken);

        if (!brandExists)
            throw new InvalidOperationException("Brand not found.");

        var product = new Product(
            request.Name,
            request.CategoryId,
            request.BrandId,
            request.Description,
            request.ImageUrl);

        foreach (var variant in request.Variants)
        {
            product.AddVariant(
                variant.OriginalPrice,
                variant.StockQuantity,
                variant.AttributeValueIds);
        }

        _context.Products.Add(product);

        await _context.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}