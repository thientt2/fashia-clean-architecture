using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Entities;
using Fashia.Domain.ValueObjects;
using MediatR;

namespace Fashia.Application.Products.Commands.CreateProduct;

public sealed record CreateProductCommand : IRequest<int>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;

    public int CategoryId { get; init; }
    public int BrandId { get; init; }

    public List<int> UploadedImageIds { get; init; } = [];
    public List<CreateProductVariantDto> Variants { get; init; } = [];
}

public sealed record CreateProductVariantDto
{
    public long OriginalPrice { get; init; }
    public List<int> UploadedImageIds { get; init; } = [];
    public List<int> AttributeValueIds { get; init; } = [];
}

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateProductCommandHandler(IApplicationDbContext context) => _context = context; // D — bỏ IUser

    public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        // G — fetch tất cả uploaded files để MarkAsUsed sau
        var allImageIds = request
            .UploadedImageIds.Concat(request.Variants.SelectMany(v => v.UploadedImageIds))
            .Distinct()
            .ToList();

        var uploadedFiles = await _context
            .UploadedFiles.Where(x => allImageIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        // A — constructor enforce ảnh bắt buộc
        var product = new Product(
            request.Name,
            request.CategoryId,
            request.BrandId,
            request.Description,
            request.UploadedImageIds
        ); // ← Domain tự xử lý isMain và displayOrder

        foreach (var variantDto in request.Variants)
        {
            // E — dùng return value, không còn Variants.Last()
            var variant = product.AddVariant(
                Money.Create(variantDto.OriginalPrice),
                variantDto.AttributeValueIds
            );

            // F — loop với đúng displayOrder, isMain do AddImage tự xử lý
            var order = 0;
            foreach (var imageId in variantDto.UploadedImageIds)
            {
                variant.AddImage(imageId, displayOrder: order);
                order++;
            }
        }

        // G — MarkAsUsed trong cùng transaction
        foreach (var imageId in allImageIds)
        {
            if (uploadedFiles.TryGetValue(imageId, out var file))
                file.MarkAsUsed();
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
