using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Entities;
using Fashia.Domain.Enums;
using Fashia.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Fashia.Application.Orders.Commands.CheckoutOrder;

public sealed record CheckoutOrderCommand : IRequest<int>
{
    public List<int> CartItemIds { get; init; } = [];
    public int ShippingAddressId { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public string? Note { get; init; }
}

public sealed class CheckoutOrderCommandHandler : IRequestHandler<CheckoutOrderCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CheckoutOrderCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<int> Handle(CheckoutOrderCommand request, CancellationToken cancellationToken)
    {
        var requestedCartItemIds = request.CartItemIds.Where(x => x > 0).Distinct().ToList();

        if (requestedCartItemIds.Count == 0)
            throw new ValidationException("At least one cart item is required.");

        var customer = await _context.Customers.FirstOrDefaultAsync(
            x => x.UserId == _user.Id,
            cancellationToken
        );

        if (customer is null)
            throw new UnauthorizedAccessException("Customer account is required.");

        var cart = await _context
            .Carts.Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.CustomerId == customer.Id, cancellationToken);

        if (cart is null)
            throw new ValidationException("Cart not found.");

        var selectedCartItems = cart.Items.Where(x => requestedCartItemIds.Contains(x.Id)).ToList();

        if (selectedCartItems.Count != requestedCartItemIds.Count)
        {
            throw new ValidationException(
                "One or more selected cart items do not belong to the current customer's cart."
            );
        }

        var shippingAddress = await _context.CustomerAddresses.FirstOrDefaultAsync(
            x => x.Id == request.ShippingAddressId && x.CustomerId == customer.Id,
            cancellationToken
        );

        if (shippingAddress is null)
            throw new ValidationException("Shipping address not found.");

        var variantIds = selectedCartItems.Select(x => x.ProductVariantId).Distinct().ToList();

        var variants = await _context
            .ProductVariants.Include(x => x.Product)
            .Include(x => x.AttributeValues)
                .ThenInclude(x => x.AttributeValue)
            .Where(x => variantIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (variants.Count != variantIds.Count)
            throw new ValidationException("One or more product variants do not exist.");

        foreach (var variant in variants)
        {
            if (variant.Product.Status != ProductStatus.Active)
            {
                throw new ValidationException($"Product variant {variant.Id} is not available.");
            }
        }

        var requestedQuantities = selectedCartItems
            .GroupBy(x => x.ProductVariantId)
            .ToDictionary(x => x.Key, x => x.Sum(i => i.Quantity));

        var branchInventories = await _context
            .BranchVariantInventories.Include(x => x.Branch)
            .Where(x => variantIds.Contains(x.ProductVariantId))
            .ToListAsync(cancellationToken);

        var branchId = branchInventories
            .Where(x => x.Branch.Status == BranchStatus.Active)
            .GroupBy(x => x.BranchId)
            .Where(group =>
                requestedQuantities.All(requested =>
                    group.Any(inventory =>
                        inventory.ProductVariantId == requested.Key
                        && inventory.AvailableQuantity >= requested.Value
                    )
                )
            )
            .OrderBy(group => group.Key)
            .Select(group => (int?)group.Key)
            .FirstOrDefault();

        if (branchId is null)
            throw new ValidationException("Insufficient inventory.");

        var orderBuilder = Order
            .CreateBuilder()
            .ForCustomer(
                customer.Id,
                shippingAddress.CustomerName,
                customer.CustomerEmail,
                shippingAddress.CustomerPhone
            )
            .FromBranch(branchId.Value)
            .ShipTo(shippingAddress.Address.Copy())
            .PaidBy(request.PaymentMethod)
            .WithNote(request.Note);

        foreach (var cartItem in selectedCartItems)
        {
            var variant = variants.Single(x => x.Id == cartItem.ProductVariantId);

            var inventory = branchInventories.Single(x =>
                x.BranchId == branchId.Value && x.ProductVariantId == cartItem.ProductVariantId
            );

            var previousStockQuantity = inventory.StockQuantity;
            var previousReservedQuantity = inventory.ReservedQuantity;

            inventory.DecreaseStock(cartItem.Quantity);

            orderBuilder.AddItem(variant.Id, variant.SellingPrice, cartItem.Quantity);

            _context.InventoryTransactions.Add(
                new InventoryTransaction(
                    branchId.Value,
                    variant.Id,
                    InventoryTransactionType.Sale,
                    cartItem.Quantity,
                    "Checkout order sale",
                    previousStockQuantity: previousStockQuantity,
                    newStockQuantity: inventory.StockQuantity,
                    previousReservedQuantity: previousReservedQuantity,
                    newReservedQuantity: inventory.ReservedQuantity
                )
            );

            cart.RemoveItem(cartItem.Id);
        }

        var order = orderBuilder.Build();

        _context.Orders.Add(order);

        await _context.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}
