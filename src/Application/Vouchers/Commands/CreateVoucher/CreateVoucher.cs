using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Entities;
using Fashia.Domain.Enums;

namespace Fashia.Application.Vouchers.Commands.CreateVoucher;

public sealed record CreateVoucherCommand : IRequest<int>
{
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

public sealed class CreateVoucherCommandHandler : IRequestHandler<CreateVoucherCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateVoucherCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(
        CreateVoucherCommand request,
        CancellationToken cancellationToken
    )
    {
        var code = request.Code.Trim().ToUpperInvariant();
        var codeExists = await _context.Vouchers.AnyAsync(x => x.Code == code, cancellationToken);

        if (codeExists)
            throw new InvalidOperationException("Voucher code already exists.");

        var voucher = new Voucher(
            code,
            request.DiscountAmount,
            request.ValidFrom,
            request.ValidUntil,
            request.VoucherType,
            request.DiscountType,
            request.Display,
            request.QuantityPerUser,
            request.UsageLimit,
            request.MinOrderAmount,
            request.MaxDiscountAmount
        );

        _context.Vouchers.Add(voucher);

        await _context.SaveChangesAsync(cancellationToken);

        return voucher.Id;
    }
}
