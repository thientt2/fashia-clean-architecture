using Fashia.Application.Common.Interfaces;

namespace Fashia.Application.Vouchers.Queries;

public sealed record GetOrderVoucherUsagesQuery(int? VoucherId = null, int? OrderId = null)
    : IRequest<IReadOnlyCollection<OrderVoucherUsageDto>>;

public sealed class GetOrderVoucherUsagesQueryHandler
    : IRequestHandler<GetOrderVoucherUsagesQuery, IReadOnlyCollection<OrderVoucherUsageDto>>
{
    private readonly IApplicationDbContext _context;

    public GetOrderVoucherUsagesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<OrderVoucherUsageDto>> Handle(
        GetOrderVoucherUsagesQuery request,
        CancellationToken cancellationToken
    )
    {
        var query = _context.OrderVouchers.AsNoTracking();

        if (request.VoucherId.HasValue)
            query = query.Where(x => x.VoucherId == request.VoucherId.Value);

        if (request.OrderId.HasValue)
            query = query.Where(x => x.OrderId == request.OrderId.Value);

        return await query
            .OrderByDescending(x => x.AppliedAt)
            .Select(x => new OrderVoucherUsageDto
            {
                Id = x.Id,
                OrderId = x.OrderId,
                CustomerId = x.Order.CustomerId,
                CustomerName = x.Order.CustomerName,
                VoucherId = x.VoucherId,
                VoucherCode = x.VoucherCode,
                OrderSubTotalAmount = x.Order.SubTotalAmount,
                DiscountAmount = x.DiscountAmount,
                OrderTotalAmount = x.Order.TotalAmount,
                AppliedAt = x.AppliedAt,
            })
            .ToListAsync(cancellationToken);
    }
}
