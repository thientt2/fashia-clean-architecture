using Fashia.Application.Common.Exceptions;
using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Entities;
using Fashia.Domain.Enums;

namespace Fashia.Application.Inventories.Commands.TransferInventoryStock;

public sealed record TransferInventoryStockCommand : IRequest
{
    public int SourceBranchId { get; init; }
    public int DestinationBranchId { get; init; }
    public int ProductVariantId { get; init; }
    public int Quantity { get; init; }
    public string? Note { get; init; }
}

public sealed class TransferInventoryStockCommandHandler
    : IRequestHandler<TransferInventoryStockCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IBranchAuthorizationService _branchAuthorizationService;

    public TransferInventoryStockCommandHandler(
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
        TransferInventoryStockCommand request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(_user.Id))
            throw new UnauthorizedAccessException();

        if (request.SourceBranchId == request.DestinationBranchId)
            throw new InvalidOperationException(
                "Source and destination branches must be different."
            );

        await AuthorizeBranchAsync(request.SourceBranchId, cancellationToken);
        await AuthorizeBranchAsync(request.DestinationBranchId, cancellationToken);

        var branches = await _context
            .Branches.Where(x =>
                x.Id == request.SourceBranchId || x.Id == request.DestinationBranchId
            )
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (!branches.Contains(request.SourceBranchId))
            throw new InvalidOperationException("Source branch not found.");

        if (!branches.Contains(request.DestinationBranchId))
            throw new InvalidOperationException("Destination branch not found.");

        var variantExists = await _context.ProductVariants.AnyAsync(
            x => x.Id == request.ProductVariantId,
            cancellationToken
        );

        if (!variantExists)
            throw new InvalidOperationException("Product variant not found.");

        var sourceInventory = await _context.BranchVariantInventories.FirstOrDefaultAsync(
            x =>
                x.BranchId == request.SourceBranchId
                && x.ProductVariantId == request.ProductVariantId,
            cancellationToken
        );

        if (sourceInventory is null)
            throw new InvalidOperationException("Source inventory not found.");

        var destinationInventory = await _context.BranchVariantInventories.FirstOrDefaultAsync(
            x =>
                x.BranchId == request.DestinationBranchId
                && x.ProductVariantId == request.ProductVariantId,
            cancellationToken
        );

        if (destinationInventory is null)
        {
            destinationInventory = BranchVariantInventory.Create(
                request.DestinationBranchId,
                request.ProductVariantId
            );
            _context.BranchVariantInventories.Add(destinationInventory);
        }

        var sourcePreviousStockQuantity = sourceInventory.StockQuantity;
        var sourcePreviousReservedQuantity = sourceInventory.ReservedQuantity;
        var destinationPreviousStockQuantity = destinationInventory.StockQuantity;
        var destinationPreviousReservedQuantity = destinationInventory.ReservedQuantity;
        var transferCorrelationId = Guid.NewGuid();

        sourceInventory.DecreaseStock(request.Quantity);
        destinationInventory.IncreaseStock(request.Quantity);

        _context.InventoryTransactions.Add(
            new InventoryTransaction(
                request.SourceBranchId,
                request.ProductVariantId,
                InventoryTransactionType.TransferOut,
                request.Quantity,
                request.Note,
                previousStockQuantity: sourcePreviousStockQuantity,
                newStockQuantity: sourceInventory.StockQuantity,
                previousReservedQuantity: sourcePreviousReservedQuantity,
                newReservedQuantity: sourceInventory.ReservedQuantity,
                sourceBranchId: request.SourceBranchId,
                destinationBranchId: request.DestinationBranchId,
                transferCorrelationId: transferCorrelationId
            )
        );

        _context.InventoryTransactions.Add(
            new InventoryTransaction(
                request.DestinationBranchId,
                request.ProductVariantId,
                InventoryTransactionType.TransferIn,
                request.Quantity,
                request.Note,
                previousStockQuantity: destinationPreviousStockQuantity,
                newStockQuantity: destinationInventory.StockQuantity,
                previousReservedQuantity: destinationPreviousReservedQuantity,
                newReservedQuantity: destinationInventory.ReservedQuantity,
                sourceBranchId: request.SourceBranchId,
                destinationBranchId: request.DestinationBranchId,
                transferCorrelationId: transferCorrelationId
            )
        );

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task AuthorizeBranchAsync(int branchId, CancellationToken cancellationToken)
    {
        var canManageBranch = await _branchAuthorizationService.CanManageBranchAsync(
            _user.Id!,
            branchId,
            cancellationToken
        );

        if (!canManageBranch)
            throw new ForbiddenAccessException();
    }
}
