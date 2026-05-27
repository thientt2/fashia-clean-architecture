using Fashia.Application.Common.Interfaces;
using Fashia.Application.Common.Models;

namespace Fashia.Application.Products.Queries;

public sealed record GetProductsQuery : IRequest<IReadOnlyCollection<ProductDto>>;

public sealed record GetProductByIdQuery(int Id) : IRequest<ProductDto?>;
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
        CancellationToken cancellationToken)
    {
        return await _context.Products
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new ProductDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ImageUrl = x.ImageUrl,
                Category = new LookupDto { Id = x.Category.Id, Name = x.Category.Name },
                Brand = new LookupDto { Id = x.Brand.Id, Name = x.Brand.Name },
                Status = x.Status.ToString(),
                Variants = x.Variants
                    .OrderBy(v => v.Id)
                    .Select(v => new ProductVariantDto
                    {
                        Id = v.Id,
                        OriginalPrice = v.OriginalPrice,
                        DiscountPercentage = v.DiscountPercentage,
                        StockQuantity = v.StockQuantity,
                        SellingPrice = v.OriginalPrice * (1 - v.DiscountPercentage / 100),
                        AttributeValues = v.AttributeValues
                            .OrderBy(av => av.AttributeValue.Value)
                            .Select(av => new ProductVariantAttributeValueDto
                            {
                                Id = av.AttributeValueId,
                                Value = av.AttributeValue.Value,
                                HexValue = av.AttributeValue.HexValue
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetProductByIdQueryHandler
    : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IApplicationDbContext _context;

    public GetProductByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProductDto?> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new ProductDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ImageUrl = x.ImageUrl,
                Category = new LookupDto { Id = x.Category.Id, Name = x.Category.Name },
                Brand = new LookupDto { Id = x.Brand.Id, Name = x.Brand.Name },
                Status = x.Status.ToString(),
                Variants = x.Variants
                    .OrderBy(v => v.Id)
                    .Select(v => new ProductVariantDto
                    {
                        Id = v.Id,
                        OriginalPrice = v.OriginalPrice,
                        DiscountPercentage = v.DiscountPercentage,
                        StockQuantity = v.StockQuantity,
                        SellingPrice = v.OriginalPrice * (1 - v.DiscountPercentage / 100),
                        AttributeValues = v.AttributeValues
                            .OrderBy(av => av.AttributeValue.Value)
                            .Select(av => new ProductVariantAttributeValueDto
                            {
                                Id = av.AttributeValueId,
                                Value = av.AttributeValue.Value,
                                HexValue = av.AttributeValue.HexValue
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
