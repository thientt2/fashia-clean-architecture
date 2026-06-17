using Fashia.Application.Common.Exceptions;
using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Entities;
using Fashia.Domain.Enums;

namespace Fashia.Application.Inventories.Commands.AdjustInventoryStock;

public sealed record AdjustInventoryStockCommand : IRequest
{
    public int BranchId { get; init; }
    public int ProductVariantId { get; init; }
    public int Quantity { get; init; }
    public string? Note { get; init; }
}

public sealed class AdjustInventoryStockCommandHandler
    : IRequestHandler<AdjustInventoryStockCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IBranchAuthorizationService _branchAuthorizationService;

    public AdjustInventoryStockCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IBranchAuthorizationService branchAuthorizationService
    )
    {
        _context = context;
        _user = user;
        _branchAuthorizationService = branchAuthorizationService;
    }

    public async Task Handle(
        AdjustInventoryStockCommand request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(_user.Id))
            throw new UnauthorizedAccessException();

        var canManageBranch = await _branchAuthorizationService.CanManageBranchAsync(
            _user.Id,
            request.BranchId,
            cancellationToken
        );

        if (!canManageBranch)
            throw new ForbiddenAccessException();

        var inventory = await _context.BranchVariantInventories.FirstOrDefaultAsync(
            x => x.BranchId == request.BranchId && x.ProductVariantId == request.ProductVariantId,
            cancellationToken
        );

        if (inventory is null)
            throw new InvalidOperationException("Inventory not found.");

        var previousStockQuantity = inventory.StockQuantity;
        var previousReservedQuantity = inventory.ReservedQuantity;
        var adjustmentQuantity = Math.Abs(request.Quantity - previousStockQuantity);

        inventory.AdjustStock(request.Quantity);

        if (adjustmentQuantity == 0)
            return;

        _context.InventoryTransactions.Add(
            new InventoryTransaction(
                request.BranchId,
                request.ProductVariantId,
                InventoryTransactionType.Adjustment,
                adjustmentQuantity,
                request.Note,
                previousStockQuantity: previousStockQuantity,
                newStockQuantity: inventory.StockQuantity,
                previousReservedQuantity: previousReservedQuantity,
                newReservedQuantity: inventory.ReservedQuantity
            )
        );

        await _context.SaveChangesAsync(cancellationToken);
    }
}
