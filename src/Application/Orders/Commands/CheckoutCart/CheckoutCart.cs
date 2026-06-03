using Fashia.Application.Carts.Common;
using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Entities;
using Fashia.Domain.Enums;

namespace Fashia.Application.Orders.Commands.CheckoutCart;

public sealed record CheckoutCartCommand : IRequest<int>
{
    public int BranchId { get; init; }
    public string? VoucherCode { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string? CustomerEmail { get; init; }
    public string CustomerPhone { get; init; } = string.Empty;
    public string ShippingAddress { get; init; } = string.Empty;
}

public sealed class CheckoutCartCommandHandler : IRequestHandler<CheckoutCartCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CheckoutCartCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<int> Handle(CheckoutCartCommand request, CancellationToken cancellationToken)
    {
        var customerId = await CartHelpers.GetCurrentCustomerIdAsync(
            _context,
            _user,
            cancellationToken
        );

        if (!customerId.HasValue)
            throw new UnauthorizedAccessException("Authentication cookie is required.");

        var cart = await GetCustomerCartAsync(customerId.Value, cancellationToken);

        if (cart is null)
            throw new InvalidOperationException("Cart not found.");

        if (cart.Items.Count == 0)
            throw new InvalidOperationException("Cart is empty.");

        var branchExists = await _context.Branches.AnyAsync(
            x => x.Id == request.BranchId,
            cancellationToken
        );

        if (!branchExists)
            throw new InvalidOperationException("Branch not found.");

        var requestedItems = cart
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

        var order = new Order(
            customerId,
            request.BranchId,
            request.CustomerName,
            request.CustomerEmail,
            request.CustomerPhone,
            request.ShippingAddress
        );

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
                    "Order sale"
                )
            );
        }

        await ApplyVoucherAsync(order, customerId, request.VoucherCode, cancellationToken);

        cart.Checkout();
        _context.Orders.Add(order);

        await _context.SaveChangesAsync(cancellationToken);

        return order.Id;
    }

    private async Task<Cart?> GetCustomerCartAsync(int customerId, CancellationToken cancellationToken)
    {
        return await _context.Carts.Include(x => x.Items)
            .FirstOrDefaultAsync(
                x => x.CustomerId == customerId && x.Status == CartStatus.Active,
                cancellationToken
            );
    }

    private async Task ApplyVoucherAsync(
        Order order,
        int? customerId,
        string? voucherCode,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(voucherCode))
            return;

        var code = voucherCode.Trim().ToUpperInvariant();

        var voucher = await _context.Vouchers.FirstOrDefaultAsync(
            x => x.Code == code,
            cancellationToken
        );

        if (voucher is null)
            throw new InvalidOperationException("Voucher not found.");

        if (customerId.HasValue)
        {
            var CustomerVoucher = await _context.CustomerVouchers.FirstOrDefaultAsync(
                x => x.CustomerId == customerId.Value && x.VoucherId == voucher.Id && !x.IsRedeemed,
                cancellationToken
            );

            if (CustomerVoucher is not null)
            {
                CustomerVoucher.Redeem();
            }
            else if (voucher.QuantityPerUser > 0)
            {
                var customerUsageCount = await _context.OrderVouchers.CountAsync(
                    x => x.Order.CustomerId == customerId.Value && x.VoucherId == voucher.Id,
                    cancellationToken
                );

                if (customerUsageCount >= voucher.QuantityPerUser)
                    throw new InvalidOperationException(
                        "Customer voucher usage limit has been reached."
                    );
            }
        }

        var discountAmount = voucher.CalculateDiscount(order.SubTotalAmount);

        order.ApplyVoucher(voucher.Id, voucher.Code, discountAmount);
        voucher.MarkUsed();
    }
}
