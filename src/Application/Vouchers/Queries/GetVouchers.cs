using Fashia.Application.Common.Interfaces;

namespace Fashia.Application.Vouchers.Queries;

public sealed record GetVouchersQuery : IRequest<IReadOnlyCollection<VoucherDto>>;

public sealed record GetVoucherByIdQuery(int Id) : IRequest<VoucherDto?>;

public sealed class GetVouchersQueryHandler
    : IRequestHandler<GetVouchersQuery, IReadOnlyCollection<VoucherDto>>
{
    private readonly IApplicationDbContext _context;

    public GetVouchersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<VoucherDto>> Handle(
        GetVouchersQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _context
            .Vouchers.AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Select(x => new VoucherDto
            {
                Id = x.Id,
                Code = x.Code,
                DiscountType = x.DiscountType.ToString(),
                DiscountAmount = x.DiscountAmount,
                MinOrderAmount = x.MinOrderAmount,
                MaxDiscountAmount = x.MaxDiscountAmount,
                UsageLimit = x.UsageLimit,
                UsedCount = x.UsedCount,
                ValidFrom = x.ValidFrom,
                ValidUntil = x.ValidUntil,
                VoucherType = x.VoucherType.ToString(),
                ProductId = x.ProductId,
                CategoryId = x.CategoryId,
                BrandId = x.BrandId,
                Status = x.Status.ToString(),
                Display = x.Display.ToString(),
                QuantityPerUser = x.QuantityPerUser,
            })
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetVoucherByIdQueryHandler : IRequestHandler<GetVoucherByIdQuery, VoucherDto?>
{
    private readonly IApplicationDbContext _context;

    public GetVoucherByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<VoucherDto?> Handle(
        GetVoucherByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _context
            .Vouchers.AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new VoucherDto
            {
                Id = x.Id,
                Code = x.Code,
                DiscountType = x.DiscountType.ToString(),
                DiscountAmount = x.DiscountAmount,
                MinOrderAmount = x.MinOrderAmount,
                MaxDiscountAmount = x.MaxDiscountAmount,
                UsageLimit = x.UsageLimit,
                UsedCount = x.UsedCount,
                ValidFrom = x.ValidFrom,
                ValidUntil = x.ValidUntil,
                VoucherType = x.VoucherType.ToString(),
                ProductId = x.ProductId,
                CategoryId = x.CategoryId,
                BrandId = x.BrandId,
                Status = x.Status.ToString(),
                Display = x.Display.ToString(),
                QuantityPerUser = x.QuantityPerUser,
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
