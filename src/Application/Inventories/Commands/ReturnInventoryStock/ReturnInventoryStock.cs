using Fashia.Application.Common.Exceptions;
using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Entities;
using Fashia.Domain.Enums;

namespace Fashia.Application.Inventories.Commands.ReturnInventoryStock;

public sealed record ReturnInventoryStockCommand : IRequest
{
    public int BranchId { get; init; }
    public int ProductVariantId { get; init; }
    public int? OrderId { get; init; }
    public int Quantity { get; init; }
    public string? Note { get; init; }
}

public sealed class ReturnInventoryStockCommandHandler
    : IRequestHandler<ReturnInventoryStockCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IBranchAuthorizationService _branchAuthorizationService;

    public ReturnInventoryStockCommandHandler(
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
        ReturnInventoryStockCommand request,
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

        if (request.OrderId.HasValue)
        {
            var orderExists = await _context.Orders.AnyAsync(
                x => x.Id == request.OrderId.Value,
                cancellationToken
            );

            if (!orderExists)
                throw new InvalidOperationException("Order not found.");
        }

        var inventory = await _context.BranchVariantInventories.FirstOrDefaultAsync(
            x => x.BranchId == request.BranchId && x.ProductVariantId == request.ProductVariantId,
            cancellationToken
        );

        if (inventory is null)
            throw new InvalidOperationException("Inventory not found.");

        var previousStockQuantity = inventory.StockQuantity;
        var previousReservedQuantity = inventory.ReservedQuantity;

        inventory.ReturnStock(request.Quantity);

        _context.InventoryTransactions.Add(
            new InventoryTransaction(
                request.BranchId,
                request.ProductVariantId,
                InventoryTransactionType.Return,
                request.Quantity,
                request.Note,
                previousStockQuantity: previousStockQuantity,
                newStockQuantity: inventory.StockQuantity,
                previousReservedQuantity: previousReservedQuantity,
                newReservedQuantity: inventory.ReservedQuantity,
                orderId: request.OrderId
            )
        );

        await _context.SaveChangesAsync(cancellationToken);
    }
}
