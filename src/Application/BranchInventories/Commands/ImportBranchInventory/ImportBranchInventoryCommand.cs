using Fashia.Application.Common.Exceptions;
using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Entities;
using Fashia.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fashia.Application.BranchInventories.Commands.ImportBranchInventory;

public record ImportBranchInventoryCommand : IRequest
{
    public int BranchId { get; init; }
    public int ProductVariantId { get; init; }
    public int Quantity { get; init; }
    public string? Note { get; init; }
}

public class ImportBranchInventoryCommandHandler : IRequestHandler<ImportBranchInventoryCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public ImportBranchInventoryCommandHandler(
        IApplicationDbContext context,
        IUser user,
        IIdentityService identityService
    )
    {
        _context = context;
        _user = user;
        _identityService = identityService;
    }

    public async Task Handle(
        ImportBranchInventoryCommand request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(_user.Id))
            throw new UnauthorizedAccessException();

        var canManageBranch = await _identityService.CanManageBranchAsync(
            _user.Id,
            request.BranchId
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

        var inventory = await _context.BranchVariantInventories.FirstOrDefaultAsync(
            x => x.BranchId == request.BranchId && x.ProductVariantId == request.ProductVariantId,
            cancellationToken
        );

        if (inventory is null)
        {
            inventory = new BranchVariantInventory(request.BranchId, request.ProductVariantId);

            _context.BranchVariantInventories.Add(inventory);
        }

        inventory.IncreaseStock(request.Quantity);

        var transaction = new InventoryTransaction(
            request.BranchId,
            request.ProductVariantId,
            InventoryTransactionType.Import,
            request.Quantity,
            request.Note
        );

        _context.InventoryTransactions.Add(transaction);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
