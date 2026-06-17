using Fashia.Application.Common.Interfaces;
using Fashia.Application.Common.Models;
using Fashia.Application.Products.Queries.Common;

namespace Fashia.Application.Products.Queries.GetProductByIdQuery;

public sealed record GetProductByIdQuery(int Id) : IRequest<ProductDto?>;

public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IApplicationDbContext _context;

    public GetProductByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDto?> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var product = await _context
            .Products.AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Brand)
            .Include(x => x.Variants)
                .ThenInclude(v => v.Images)
                    .ThenInclude(i => i.UploadedFile)
            .Include(x => x.Variants)
                .ThenInclude(v => v.AttributeValues)
                    .ThenInclude(av => av.AttributeValue)
            .Include(x => x.Images)
                .ThenInclude(i => i.UploadedFile)
            .Where(x => x.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
            return null;

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            ImageUrls = product
                .Images.Select(i => new ImageDto
                {
                    UploadedFileId = i.UploadedFile.Id,
                    Url = i.UploadedFile.Url,
                    IsMain = i.IsMain,
                    DisplayOrder = i.DisplayOrder,
                })
                .ToList(),
            Category = new LookupDto { Id = product.Category.Id, Name = product.Category.Name },
            Brand = new LookupDto { Id = product.Brand.Id, Name = product.Brand.Name },
            Status = product.Status.ToString(),
            Variants = product
                .Variants.OrderBy(v => v.Id)
                .Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    OriginalPrice = v.OriginalPrice.Amount,
                    DiscountPercentage = v.DiscountPercentage.BasisPoints,
                    SellingPrice = v.SellingPrice.Amount, // ← Domain property, tính trong memory
                    AttributeValues = v
                        .AttributeValues.OrderBy(av => av.AttributeValue.Value)
                        .Select(av => new ProductVariantAttributeValueDto
                        {
                            Id = av.AttributeValueId,
                            Value = av.AttributeValue.Value,
                            HexValue = av.AttributeValue.HexValue,
                        })
                        .ToList(),
                    ImageUrls = v
                        .Images.Select(i => new ImageDto
                        {
                            UploadedFileId = i.UploadedFile.Id,
                            Url = i.UploadedFile.Url,
                            IsMain = i.IsMain,
                            DisplayOrder = i.DisplayOrder,
                        })
                        .ToList(),
                })
                .ToList(),
        };
    }
}
