namespace Fashia.Domain.Entities;

public class OrderItem : BaseAuditableEntity
{
    public int OrderId { get; private set; }
    public Order Order { get; private set; } = null!;

    public int ProductVariantId { get; private set; }
    public ProductVariant ProductVariant { get; private set; } = null!;

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal LineTotal => Quantity * UnitPrice;

    private OrderItem()
    {
        // EF Core
    }

    public OrderItem(int productVariantId, int quantity, decimal unitPrice)
    {
        SetProductVariantId(productVariantId);
        SetQuantity(quantity);
        SetUnitPrice(unitPrice);
    }

    public void IncreaseQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        Quantity += quantity;
    }

    private void SetProductVariantId(int productVariantId)
    {
        if (productVariantId <= 0)
            throw new ArgumentException(
                "Product variant id is required.",
                nameof(productVariantId)
            );

        ProductVariantId = productVariantId;
    }

    private void SetQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        Quantity = quantity;
    }

    private void SetUnitPrice(decimal unitPrice)
    {
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

        UnitPrice = decimal.Round(unitPrice, 2, MidpointRounding.AwayFromZero);
    }
}
