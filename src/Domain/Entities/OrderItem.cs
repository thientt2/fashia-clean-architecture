using Fashia.Domain.Common;
using Fashia.Domain.ValueObjects;

namespace Fashia.Domain.Entities;

public sealed class OrderItem : BaseEntity
{
    public int OrderId { get; private set; }
    public Order Order { get; private set; } = null!;

    public int ProductVariantId { get; private set; }
    public ProductVariant ProductVariant { get; private set; } = null!;

    public string ProductVariantName { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public string? VariantName { get; private set; }
    public string? Sku { get; private set; }

    public Money UnitPrice { get; private set; } = default!;

    public int Quantity { get; private set; }

    public Money LineTotal => UnitPrice.Multiply(Quantity);

    private OrderItem()
    {
        // EF Core
    }

    /*
    Legacy constructor kept for review history. It is wrong for order history
    because it stores only variant id, quantity, and decimal price. The current
    model stores a product snapshot and Money value object instead.

    public OrderItem(int productVariantId, int quantity, decimal unitPrice)
    {
        SetProductVariantId(productVariantId);
        SetQuantity(quantity);
        SetUnitPrice(unitPrice);
    }
    */

    private OrderItem(int productVariantId, int quantity, Money unitPrice)
    {
        if (productVariantId <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(productVariantId),
                "Product variant ID must be greater than zero."
            );
        ProductVariantId = productVariantId;
        SetQuantity(quantity);
        SetUnitPrice(unitPrice);
    }

    public static OrderItem Create(int productVariantId, Money unitPrice, int quantity)
    {
        return new OrderItem(
            productVariantId: productVariantId,
            unitPrice: unitPrice,
            quantity: quantity
        );
    }

    public void IncreaseQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Quantity must be greater than zero."
            );

        Quantity += quantity;
    }

    public void ChangeQuantity(int quantity)
    {
        SetQuantity(quantity);
    }

    private void SetUnitPrice(Money unitPrice)
    {
        UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
    }

    private void SetQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Quantity must be greater than zero."
            );

        Quantity = quantity;
    }
}
