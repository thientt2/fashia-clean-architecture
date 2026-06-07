using Fashia.Application.Common.Interfaces;
using Fashia.Application.Common.Models;
using Fashia.Application.Products.Queries.Common;

namespace Fashia.Application.Products.Queries.GetProductsQuery;

public sealed record GetProductsQuery : IRequest<IReadOnlyCollection<ProductDto>>;

public sealed class GetProductsQueryHandler
    : IRequestHandler<GetProductsQuery, IReadOnlyCollection<ProductDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<ProductDto>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _context
            .Products.AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new ProductDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ImageUrls = x
                    .Images.Select(i => new ImageDto
                    {
                        UploadedFileId = i.UploadedFile.Id,
                        Url = i.UploadedFile.Url,
                        IsMain = i.IsMain,
                        DisplayOrder = i.DisplayOrder,
                    })
                    .ToList(),
                Category = new LookupDto { Id = x.Category.Id, Name = x.Category.Name },
                Brand = new LookupDto { Id = x.Brand.Id, Name = x.Brand.Name },
                Status = x.Status.ToString(),
                Variants = x
                    .Variants.OrderBy(v => v.Id)
                    .Select(v => new ProductVariantDto
                    {
                        Id = v.Id,
                        OriginalPrice = v.OriginalPrice.Amount,
                        DiscountPercentage = v.DiscountPercentage.Value,
                        SellingPrice =
                            v.OriginalPrice.Amount * (1 - v.DiscountPercentage.Value / 100),
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
            })
            .ToListAsync(cancellationToken);
    }
}
