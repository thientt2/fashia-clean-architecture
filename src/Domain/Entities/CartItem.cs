namespace Fashia.Domain.Entities;

public class CartItem : BaseAuditableEntity
{
    public int CartId { get; private set; }
    public Cart Cart { get; private set; } = null!;

    public int ProductVariantId { get; private set; }
    public ProductVariant ProductVariant { get; private set; } = null!;

    public int Quantity { get; private set; }

    private CartItem()
    {
        // EF Core
    }

    public CartItem(int productVariantId, int quantity)
    {
        SetProductVariantId(productVariantId);
        SetQuantity(quantity);
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

    private void SetProductVariantId(int productVariantId)
    {
        if (productVariantId <= 0)
            throw new ArgumentException("Product variant id is required.", nameof(productVariantId));

        ProductVariantId = productVariantId;
    }

    private void SetQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        Quantity = quantity;
    }
}
