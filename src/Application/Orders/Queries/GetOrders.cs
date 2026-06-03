using Fashia.Application.Common.Interfaces;

namespace Fashia.Application.Orders.Queries;

public sealed record GetOrdersQuery : IRequest<IReadOnlyCollection<OrderDto>>;

public sealed record GetOrderByIdQuery(int Id) : IRequest<OrderDto?>;

public sealed class GetOrdersQueryHandler
    : IRequestHandler<GetOrdersQuery, IReadOnlyCollection<OrderDto>>
{
    private readonly IApplicationDbContext _context;

    public GetOrdersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<OrderDto>> Handle(
        GetOrdersQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _context
            .Orders.AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Select(x => new OrderDto
            {
                Id = x.Id,
                CustomerId = x.CustomerId,
                CustomerName = x.CustomerName,
                CustomerEmail = x.CustomerEmail,
                CustomerPhone = x.CustomerPhone,
                ShippingAddress = x.ShippingAddress,
                BranchId = x.BranchId,
                BranchName = x.Branch.Name,
                VoucherId = x.Vouchers.Select(v => (int?)v.VoucherId).FirstOrDefault(),
                VoucherCode = x.Vouchers.Select(v => v.VoucherCode).FirstOrDefault(),
                SubTotalAmount = x.SubTotalAmount,
                DiscountAmount = x.DiscountAmount,
                TotalAmount = x.TotalAmount,
                Status = x.Status.ToString(),
                Items = x
                    .Items.OrderBy(i => i.Id)
                    .Select(i => new OrderItemDto
                    {
                        Id = i.Id,
                        ProductVariantId = i.ProductVariantId,
                        ProductName = i.ProductVariant.Product.Name,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        LineTotal = i.Quantity * i.UnitPrice,
                    })
                    .ToList(),
                Vouchers = x
                    .Vouchers.OrderBy(v => v.Id)
                    .Select(v => new OrderVoucherDto
                    {
                        Id = v.Id,
                        VoucherId = v.VoucherId,
                        VoucherCode = v.VoucherCode,
                        DiscountAmount = v.DiscountAmount,
                        AppliedAt = v.AppliedAt,
                    })
                    .ToList(),
            })
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IApplicationDbContext _context;

    public GetOrderByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrderDto?> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        return await _context
            .Orders.AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new OrderDto
            {
                Id = x.Id,
                CustomerId = x.CustomerId,
                CustomerName = x.CustomerName,
                CustomerEmail = x.CustomerEmail,
                CustomerPhone = x.CustomerPhone,
                ShippingAddress = x.ShippingAddress,
                BranchId = x.BranchId,
                BranchName = x.Branch.Name,
                VoucherId = x.Vouchers.Select(v => (int?)v.VoucherId).FirstOrDefault(),
                VoucherCode = x.Vouchers.Select(v => v.VoucherCode).FirstOrDefault(),
                SubTotalAmount = x.SubTotalAmount,
                DiscountAmount = x.DiscountAmount,
                TotalAmount = x.TotalAmount,
                Status = x.Status.ToString(),
                Items = x
                    .Items.OrderBy(i => i.Id)
                    .Select(i => new OrderItemDto
                    {
                        Id = i.Id,
                        ProductVariantId = i.ProductVariantId,
                        ProductName = i.ProductVariant.Product.Name,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        LineTotal = i.Quantity * i.UnitPrice,
                    })
                    .ToList(),
                Vouchers = x
                    .Vouchers.OrderBy(v => v.Id)
                    .Select(v => new OrderVoucherDto
                    {
                        Id = v.Id,
                        VoucherId = v.VoucherId,
                        VoucherCode = v.VoucherCode,
                        DiscountAmount = v.DiscountAmount,
                        AppliedAt = v.AppliedAt,
                    })
                    .ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
