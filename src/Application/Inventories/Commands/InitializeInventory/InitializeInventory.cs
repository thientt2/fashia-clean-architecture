using Fashia.Application.Common.Exceptions;
using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Entities;
using Fashia.Domain.Enums;

namespace Fashia.Application.Inventories.Commands.InitializeInventory;

public sealed record InitializeInventoryCommand : IRequest
{
    public int BranchId { get; init; }
    public int ProductVariantId { get; init; }
    public int Quantity { get; init; }
    public string? Note { get; init; }
}

public sealed class InitializeInventoryCommandHandler : IRequestHandler<InitializeInventoryCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IBranchAuthorizationService _branchAuthorizationService;

    public InitializeInventoryCommandHandler(
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
        InitializeInventoryCommand request,
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

        var branchExists = await _context.Branches.AnyAsync(
            x => x.Id == request.BranchId,
            cancellationToken
        );

        if (!branchExists)
            throw new InvalidOperationException("Branch not found.");

        var variantExists = await _context.ProductVariants.AnyAsync(
            x => x.Id == request.ProductVariantId,
            cancellationToken
        );

        if (!variantExists)
            throw new InvalidOperationException("Product variant not found.");

        var inventoryExists = await _context.BranchVariantInventories.AnyAsync(
            x => x.BranchId == request.BranchId && x.ProductVariantId == request.ProductVariantId,
            cancellationToken
        );

        if (inventoryExists)
            throw new InvalidOperationException("Inventory has already been initialized.");

        var inventory = BranchVariantInventory.Create(request.BranchId, request.ProductVariantId);

        if (request.Quantity > 0)
            inventory.IncreaseStock(request.Quantity);

        _context.BranchVariantInventories.Add(inventory);
        _context.InventoryTransactions.Add(
            new InventoryTransaction(
                request.BranchId,
                request.ProductVariantId,
                InventoryTransactionType.Initialize,
                request.Quantity,
                request.Note,
                previousStockQuantity: 0,
                newStockQuantity: inventory.StockQuantity,
                previousReservedQuantity: 0,
                newReservedQuantity: inventory.ReservedQuantity
            )
        );

        await _context.SaveChangesAsync(cancellationToken);
    }
}
