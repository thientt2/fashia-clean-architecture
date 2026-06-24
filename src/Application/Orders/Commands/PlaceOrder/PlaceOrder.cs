using Fashia.Application.Common.Exceptions;
using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Entities;
using Fashia.Domain.Enums;
using Fashia.Domain.ValueObjects;
using System.Text.Json.Serialization;

namespace Fashia.Application.Orders.Commands.PlaceOrder;

public sealed record PlaceOrderCommand
    : IRequest<OrderCreated>, ITransactionalRequest, IIdempotentRequest
{
    [JsonIgnore]
    public string IdempotencyKey { get; init; } = Guid.NewGuid().ToString();

    [JsonIgnore]
    public int StoredResponseStatusCode => 201;

    public PlaceOrderShippingAddressDto ShippingAddress { get; init; } = new();

    public string? VoucherCode { get; init; }

    public PaymentMethod PaymentMethod { get; init; }

    public string? Note { get; init; }
}

public sealed record PlaceOrderShippingAddressDto
{
    public string CustomerName { get; init; } = string.Empty;

    public string CustomerEmail { get; init; } = string.Empty;

    public string CustomerPhone { get; init; } = string.Empty;

    public string Line1 { get; init; } = string.Empty;

    public string Ward { get; init; } = string.Empty;

    public string District { get; init; } = string.Empty;

    public string Province { get; init; } = string.Empty;

    public decimal Latitude { get; init; }

    public decimal Longitude { get; init; }
}

public sealed record OrderCreated : IIdempotentResponse
{
    public int OrderId { get; init; }

    public OrderStatus Status { get; init; }

    public int BranchId { get; init; }

    public string BranchName { get; init; } = string.Empty;

    [JsonIgnore]
    public int ResourceId => OrderId;
}

public sealed class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, OrderCreated>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public PlaceOrderCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<OrderCreated> Handle(
        PlaceOrderCommand request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(_user.Id))
            throw new UnauthorizedAccessException("Authenticated user is required.");

        var customer = await _context.Customers.FirstOrDefaultAsync(
            x => x.UserId == _user.Id,
            cancellationToken
        );

        if (customer is null)
            throw new UnauthorizedAccessException("Customer account is required.");

        var cart = await _context
            .Carts.Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.CustomerId == customer.Id, cancellationToken);

        if (cart is null || cart.Items.Count == 0)
            throw new InvalidOperationException("Cart is required and must not be empty.");

        var requestedQuantities = cart
            .Items.GroupBy(x => x.ProductVariantId)
            .ToDictionary(x => x.Key, x => x.Sum(i => i.Quantity));

        var variantIds = requestedQuantities.Keys.ToList();

        var variants = await _context
            .ProductVariants.Include(x => x.Product)
            .Include(x => x.AttributeValues)
            .ThenInclude(x => x.AttributeValue)
            .ThenInclude(x => x.Attribute)
            .Where(x => variantIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (variants.Count != variantIds.Count)
            throw new ProductUnavailableException("One or more product variants do not exist.");

        if (
            variants.Any(x =>
                x.Status != ProductVariantStatus.Active || x.Product.Status != ProductStatus.Active
            )
        )
        {
            throw new ProductUnavailableException("One or more product variants are not available.");
        }

        var branchInventories = await _context
            .BranchVariantInventories.Include(x => x.Branch)
            .Where(x => variantIds.Contains(x.ProductVariantId))
            .ToListAsync(cancellationToken);

        var selectedBranch = branchInventories
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
            .Select(group => new
            {
                Branch = group.First().Branch,
                Inventories = group.ToList(),
                Distance = HaversineDistance.Kilometers(
                    request.ShippingAddress.Latitude,
                    request.ShippingAddress.Longitude,
                    group.First().Branch.Location.Latitude,
                    group.First().Branch.Location.Longitude
                ),
            })
            .OrderBy(x => x.Distance)
            .ThenBy(x => x.Branch.Id)
            .FirstOrDefault();

        if (selectedBranch is null)
            throw new NoFulfillableBranchException("No active branch can fulfill the order.");

        var orderBuilder = Order
            .CreateBuilder()
            .ForCustomer(
                customer.Id,
                request.ShippingAddress.CustomerName,
                EmailVO.Create(request.ShippingAddress.CustomerEmail),
                PhoneNumber.Create(request.ShippingAddress.CustomerPhone)
            )
            .FromBranch(selectedBranch.Branch.Id)
            .ShipTo(
                Address.Create(
                    request.ShippingAddress.Line1,
                    request.ShippingAddress.Ward,
                    request.ShippingAddress.District,
                    request.ShippingAddress.Province
                )
            )
            .PaidBy(request.PaymentMethod)
            .WithNote(request.Note);

        foreach (var cartItem in cart.Items)
        {
            var variant = variants.Single(x => x.Id == cartItem.ProductVariantId);
            var inventory = selectedBranch.Inventories.Single(x =>
                x.ProductVariantId == cartItem.ProductVariantId
            );

            var previousStockQuantity = inventory.StockQuantity;
            var previousReservedQuantity = inventory.ReservedQuantity;

            inventory.ReserveStock(cartItem.Quantity);

            orderBuilder.AddItemSnapshot(
                variant.Id,
                variant.Product.Name,
                BuildProductVariantName(variant),
                BuildVariantName(variant),
                BuildVariantAttributes(variant),
                sku: null,
                variant.SellingPrice,
                cartItem.Quantity
            );

            _context.InventoryTransactions.Add(
                new InventoryTransaction(
                    selectedBranch.Branch.Id,
                    variant.Id,
                    InventoryTransactionType.Reserve,
                    cartItem.Quantity,
                    "Place order reservation",
                    previousStockQuantity: previousStockQuantity,
                    newStockQuantity: inventory.StockQuantity,
                    previousReservedQuantity: previousReservedQuantity,
                    newReservedQuantity: inventory.ReservedQuantity
                )
            );
        }

        var order = orderBuilder.Build();

        if (!string.IsNullOrWhiteSpace(request.VoucherCode))
        {
            await ApplyVoucherAsync(
                order,
                customer.Id,
                variants,
                request.VoucherCode,
                cancellationToken
            );
        }

        cart.Clear();
        _context.Orders.Add(order);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InsufficientStockException(
                "Checkout inventory changed. Retry with current availability."
            );
        }

        return new OrderCreated
        {
            OrderId = order.Id,
            Status = order.Status,
            BranchId = selectedBranch.Branch.Id,
            BranchName = selectedBranch.Branch.Name,
        };
    }

    private static string BuildProductVariantName(ProductVariant variant)
    {
        var variantName = BuildVariantName(variant);

        return string.IsNullOrWhiteSpace(variantName)
            ? variant.Product.Name
            : $"{variant.Product.Name} / {variantName}";
    }

    private static string? BuildVariantName(ProductVariant variant)
    {
        var values = variant
            .AttributeValues.Select(x => x.AttributeValue.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Order()
            .ToList();

        return values.Count == 0 ? null : string.Join(" / ", values);
    }

    private static string? BuildVariantAttributes(ProductVariant variant)
    {
        var values = variant
            .AttributeValues.Select(x => $"{x.AttributeValue.Attribute.Name}: {x.AttributeValue.Value}")
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Order()
            .ToList();

        return values.Count == 0 ? null : string.Join("; ", values);
    }

    private async Task ApplyVoucherAsync(
        Order order,
        int customerId,
        IReadOnlyCollection<ProductVariant> variants,
        string voucherCode,
        CancellationToken cancellationToken
    )
    {
        var normalizedCode = voucherCode.Trim().ToUpperInvariant();
        var voucher = await _context.Vouchers.FirstOrDefaultAsync(
            x => x.Code == normalizedCode,
            cancellationToken
        );

        if (voucher is null)
            throw new InvalidVoucherException("Voucher was not found.");

        if (
            !voucher.IsApplicableTo(
                variants.Select(x => x.ProductId),
                variants.Select(x => x.Product.CategoryId),
                variants.Select(x => x.Product.BrandId)
            )
        )
        {
            throw new InvalidVoucherException("Voucher is not applicable to this cart.");
        }

        CustomerVoucher? customerVoucher = null;

        if (voucher.Display == Display.Private)
        {
            customerVoucher = await _context.CustomerVouchers.FirstOrDefaultAsync(
                x => x.CustomerId == customerId && x.VoucherId == voucher.Id,
                cancellationToken
            );

            var now = DateTime.UtcNow;
            if (
                customerVoucher is null
                || customerVoucher.Status != CustomerVoucherStatus.Active
                || customerVoucher.IsRedeemed
                || now < customerVoucher.ValidFrom
                || now > customerVoucher.ValidUntil
            )
            {
                throw new InvalidVoucherException("Voucher is not available to this customer.");
            }
        }

        if (voucher.QuantityPerUser > 0)
        {
            var customerUsageCount = await _context.Orders.CountAsync(
                x =>
                    x.CustomerId == customerId
                    && x.OrderVoucher != null
                    && x.OrderVoucher.VoucherId == voucher.Id,
                cancellationToken
            );

            if (customerUsageCount >= voucher.QuantityPerUser)
            {
                throw new InvalidVoucherException(
                    "Voucher usage limit for this customer has been reached."
                );
            }
        }

        long discountAmount;
        try
        {
            discountAmount = voucher.CalculateDiscount(order.SubTotalAmount.Amount);
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidVoucherException(exception.Message);
        }

        order.ApplyVoucher(
            voucher.Id,
            voucher.Code,
            voucher.DiscountType,
            voucher.DiscountAmount,
            Money.Create(discountAmount)
        );

        voucher.MarkUsed();
        customerVoucher?.Redeem();
    }
}
