namespace Fashia.Domain.Entities;

public class CartItem : BaseAuditableEntity
{
    public int CartId { get; private set; }
    public Cart Cart { get; private set; } = null!;

    public int ProductVariantId { get; private set; }
    public ProductVariant ProductVariant { get; private set; } = null!;

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal TotalPrice => UnitPrice * Quantity;

    private CartItem()
    {
        // EF Core
    }

    private CartItem(int productVariantId, int quantity, decimal unitPrice)
    {
        SetProductVariantId(productVariantId);
        SetQuantity(quantity);
        SetUnitPrice(unitPrice);
    }

    public static CartItem Create(int productVariantId, int quantity, decimal unitPrice)
    {
        return new CartItem(productVariantId, quantity, unitPrice);
    }

    public void IncreaseQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        Quantity += quantity;
    }

    public void UpdateQuantity(int quantity)
    {
        SetQuantity(quantity);
    }

    public void RefreshUnitPrice(decimal currentUnitPrice)
    {
        SetUnitPrice(currentUnitPrice);
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

        UnitPrice = unitPrice;
    }
}
