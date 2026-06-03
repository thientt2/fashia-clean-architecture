using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Enums;

namespace Fashia.Application.Vouchers.Commands.UpdateVoucher;

public sealed record UpdateVoucherCommand : IRequest
{
    public int Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public DiscountType DiscountType { get; init; } = DiscountType.FixedAmount;
    public int DiscountAmount { get; init; }
    public int MinOrderAmount { get; init; }
    public int MaxDiscountAmount { get; init; }
    public int UsageLimit { get; init; }
    public DateTime ValidFrom { get; init; }
    public DateTime ValidUntil { get; init; }
    public VoucherType VoucherType { get; init; } = VoucherType.All;
    public Display Display { get; init; } = Display.Public;
    public int QuantityPerUser { get; init; } = 1;
}

public sealed class UpdateVoucherCommandHandler : IRequestHandler<UpdateVoucherCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateVoucherCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateVoucherCommand request, CancellationToken cancellationToken)
    {
        var voucher = await _context.Vouchers.FirstOrDefaultAsync(
            x => x.Id == request.Id,
            cancellationToken
        );

        if (voucher is null)
            throw new InvalidOperationException("Voucher not found.");

        var code = request.Code.Trim().ToUpperInvariant();
        var codeExists = await _context.Vouchers.AnyAsync(
            x => x.Id != request.Id && x.Code == code,
            cancellationToken
        );

        if (codeExists)
            throw new InvalidOperationException("Voucher code already exists.");

        voucher.UpdateCode(code);
        voucher.UpdateDiscountType(request.DiscountType);
        voucher.UpdateDiscountAmount(request.DiscountAmount);
        voucher.UpdateMinOrderAmount(request.MinOrderAmount);
        voucher.UpdateMaxDiscountAmount(request.MaxDiscountAmount);
        voucher.UpdateUsageLimit(request.UsageLimit);
        voucher.UpdateDateRange(request.ValidFrom, request.ValidUntil);
        voucher.UpdateVoucherType(request.VoucherType);
        voucher.UpdateDisplay(request.Display);
        voucher.UpdateQuantityPerUser(request.QuantityPerUser);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
