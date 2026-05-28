using Fashia.Application.Common.Interfaces;
using Fashia.Application.Common.Models;
using Fashia.Domain.Entities;
using MediatR;

namespace Fashia.Application.Products.Commands.CreateProduct;

public sealed record CreateProductCommand : IRequest<int>
{
    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }

    public FileUpload? Image { get; init; }

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

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _fileStorageService;

    public CreateProductCommandHandler(
        IApplicationDbContext context,
        IFileStorageService fileStorageService
    )
    {
        _context = context;
        _fileStorageService = fileStorageService;
    }

    public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        string? imageUrl = null;

        try
        {
            await ValidateReferencesAsync(request, cancellationToken);
            await ValidateAttributeValuesAsync(request, cancellationToken);

            if (request.Image is not null)
            {
                imageUrl = await _fileStorageService.UploadAsync(
                    request.Image.Stream,
                    request.Image.FileName,
                    request.Image.ContentType,
                    "products",
                    cancellationToken
                );
            }

            var product = new Product(
                request.Name,
                request.CategoryId,
                request.BrandId,
                request.Description,
                imageUrl
            );

            foreach (var variant in request.Variants)
            {
                product.AddVariant(variant.OriginalPrice, variant.AttributeValueIds);
            }

            _context.Products.Add(product);

            await _context.SaveChangesAsync(cancellationToken);

            return product.Id;
        }
        catch
        {
            if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                await _fileStorageService.DeleteAsync(imageUrl, cancellationToken);
            }

            throw;
        }
    }

    private async Task ValidateReferencesAsync(
        CreateProductCommand request,
        CancellationToken cancellationToken
    )
    {
        var categoryExists = await _context.Categories.AnyAsync(
            x => x.Id == request.CategoryId,
            cancellationToken
        );

        if (!categoryExists)
            throw new InvalidOperationException("Category not found.");

        var brandExists = await _context.Brands.AnyAsync(
            x => x.Id == request.BrandId,
            cancellationToken
        );

        if (!brandExists)
            throw new InvalidOperationException("Brand not found.");
    }

    private async Task ValidateAttributeValuesAsync(
        CreateProductCommand request,
        CancellationToken cancellationToken
    )
    {
        var attributeValueIds = request
            .Variants.SelectMany(x => x.AttributeValueIds)
            .Distinct()
            .ToList();

        var existingCount = await _context.AttributeValues.CountAsync(
            x => attributeValueIds.Contains(x.Id),
            cancellationToken
        );

        if (existingCount != attributeValueIds.Count)
            throw new InvalidOperationException("One or more attribute values do not exist.");
    }
}
