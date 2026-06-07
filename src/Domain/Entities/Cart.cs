namespace Fashia.Domain.Entities;

public class Cart : BaseAuditableEntity
{
    private readonly List<CartItem> _items = new();

    public int? CustomerId { get; private set; }
    public Customer? Customer { get; private set; }
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    private Cart()
    {
        // EF Core
    }

    public Cart(int customerId)
    {
        SetCustomer(customerId);
    }

    public void AddItem(int productVariantId, int quantity)
    {
        var existingItem = _items.FirstOrDefault(x => x.ProductVariantId == productVariantId);
        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
            return;
        }

        _items.Add(new CartItem(productVariantId, quantity));
    }

    public void UpdateItemQuantity(int productVariantId, int quantity)
    {
        var item =
            _items.FirstOrDefault(x => x.ProductVariantId == productVariantId)
            ?? throw new InvalidOperationException("Cart item not found.");

        item.UpdateQuantity(quantity);
    }

    public void RemoveItem(int productVariantId)
    {
        var item = _items.FirstOrDefault(x => x.ProductVariantId == productVariantId);
        if (item is not null)
            _items.Remove(item);
    }

    public void AssignCustomer(int customerId)
    {
        if (customerId <= 0)
            throw new ArgumentException("Customer id is required.", nameof(customerId));

        CustomerId = customerId;
    }

    private void SetCustomer(int customerId)
    {
        if (customerId <= 0)
            throw new ArgumentException("Customer id is required.", nameof(customerId));

        CustomerId = customerId;
    }
}
