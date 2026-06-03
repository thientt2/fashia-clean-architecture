using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Entities;
using Fashia.Domain.Enums;

namespace Fashia.Application.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand : IRequest<int>
{
    public int CustomerId { get; init; }

    public int BranchId { get; init; }

    public string? VoucherCode { get; init; }

    public List<CreateOrderItemDto> Items { get; init; } = [];
}

public sealed record CreateOrderItemDto
{
    public int ProductVariantId { get; init; }

    public int Quantity { get; init; }
}

public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, int>
{
    private readonly IApplicationDbContext _context;

    public CreateOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        await ValidateReferencesAsync(request, cancellationToken);

        var requestedItems = request
            .Items.GroupBy(x => x.ProductVariantId)
            .Select(x => new { ProductVariantId = x.Key, Quantity = x.Sum(i => i.Quantity) })
            .ToList();

        var variantIds = requestedItems.Select(x => x.ProductVariantId).ToList();

        var variants = await _context
            .ProductVariants.AsNoTracking()
            .Where(x => variantIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (variants.Count != variantIds.Count)
            throw new InvalidOperationException("One or more product variants do not exist.");

        var inventories = await _context
            .BranchVariantInventories.Where(x =>
                x.BranchId == request.BranchId && variantIds.Contains(x.ProductVariantId)
            )
            .ToListAsync(cancellationToken);

        var order = new Order(request.CustomerId, request.BranchId);

        foreach (var item in requestedItems)
        {
            var inventory = inventories.FirstOrDefault(x =>
                x.ProductVariantId == item.ProductVariantId
            );

            if (inventory is null || inventory.StockQuantity < item.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for product variant {item.ProductVariantId}."
                );

            var variant = variants.First(x => x.Id == item.ProductVariantId);
            var unitPrice = variant.SellingPrice.Amount;

            order.AddItem(item.ProductVariantId, item.Quantity, unitPrice);
            inventory.DecreaseStock(item.Quantity);

            _context.InventoryTransactions.Add(
                new InventoryTransaction(
                    request.BranchId,
                    item.ProductVariantId,
                    InventoryTransactionType.Sale,
                    item.Quantity,
                    $"Order sale"
                )
            );
        }

        await ApplyVoucherAsync(order, request, cancellationToken);

        _context.Orders.Add(order);

        await _context.SaveChangesAsync(cancellationToken);

        return order.Id;
    }

    private async Task ValidateReferencesAsync(
        CreateOrderCommand request,
        CancellationToken cancellationToken
    )
    {
        var customerExists = await _context.Customers.AnyAsync(
            x => x.Id == request.CustomerId,
            cancellationToken
        );

        if (!customerExists)
            throw new InvalidOperationException("Customer not found.");

        var branchExists = await _context.Branches.AnyAsync(
            x => x.Id == request.BranchId,
            cancellationToken
        );

        if (!branchExists)
            throw new InvalidOperationException("Branch not found.");
    }

    private async Task ApplyVoucherAsync(
        Order order,
        CreateOrderCommand request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(request.VoucherCode))
            return;

        var code = request.VoucherCode.Trim().ToUpperInvariant();

        var voucher = await _context.Vouchers.FirstOrDefaultAsync(
            x => x.Code == code,
            cancellationToken
        );

        if (voucher is null)
            throw new InvalidOperationException("Voucher not found.");

        var CustomerVoucher = await _context
            .CustomerVouchers.Include(x => x.Voucher)
            .FirstOrDefaultAsync(
                x =>
                    x.CustomerId == request.CustomerId
                    && x.VoucherId == voucher.Id
                    && !x.IsRedeemed,
                cancellationToken
            );

        if (CustomerVoucher is not null)
        {
            CustomerVoucher.Redeem();
        }
        else if (voucher.QuantityPerUser > 0)
        {
            var customerUsageCount = await _context.OrderVouchers.CountAsync(
                x => x.Order.CustomerId == request.CustomerId && x.VoucherId == voucher.Id,
                cancellationToken
            );

            if (customerUsageCount >= voucher.QuantityPerUser)
                throw new InvalidOperationException(
                    "Customer voucher usage limit has been reached."
                );
        }

        var discountAmount = voucher.CalculateDiscount(order.SubTotalAmount);

        order.ApplyVoucher(voucher.Id, voucher.Code, discountAmount);
        voucher.MarkUsed();
    }
}
