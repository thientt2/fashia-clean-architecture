using Fashia.Application.Common.Interfaces;

namespace Fashia.Application.Vouchers.Commands.ChangeVoucherStatus;

public sealed record DeactivateVoucherCommand(int Id) : IRequest;
public sealed record SuspendVoucherCommand(int Id) : IRequest;
public sealed record ReactivateVoucherCommand(int Id) : IRequest;

public sealed class DeactivateVoucherCommandHandler : IRequestHandler<DeactivateVoucherCommand>
{
    private readonly IApplicationDbContext _context;

    public DeactivateVoucherCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeactivateVoucherCommand request, CancellationToken cancellationToken)
    {
        var voucher = await _context.Vouchers.FirstOrDefaultAsync(
            x => x.Id == request.Id,
            cancellationToken
        );

        if (voucher is null)
            throw new InvalidOperationException("Voucher not found.");

        voucher.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public sealed class SuspendVoucherCommandHandler : IRequestHandler<SuspendVoucherCommand>
{
    private readonly IApplicationDbContext _context;

    public SuspendVoucherCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(SuspendVoucherCommand request, CancellationToken cancellationToken)
    {
        var voucher = await _context.Vouchers.FirstOrDefaultAsync(
            x => x.Id == request.Id,
            cancellationToken
        );

        if (voucher is null)
            throw new InvalidOperationException("Voucher not found.");

        voucher.Suspend();
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public sealed class ReactivateVoucherCommandHandler : IRequestHandler<ReactivateVoucherCommand>
{
    private readonly IApplicationDbContext _context;

    public ReactivateVoucherCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ReactivateVoucherCommand request, CancellationToken cancellationToken)
    {
        var voucher = await _context.Vouchers.FirstOrDefaultAsync(
            x => x.Id == request.Id,
            cancellationToken
        );

        if (voucher is null)
            throw new InvalidOperationException("Voucher not found.");

        voucher.Reactivate();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
