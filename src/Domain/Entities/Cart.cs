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

    private Cart(int customerId)
    {
        CustomerId = customerId;
    }

    public static Cart Create(int customerId)
    {
        if (customerId <= 0)
            throw new ArgumentException("Customer id is required.", nameof(customerId));

        return new Cart(customerId);
    }

    public void AddItem(int productVariantId, int quantity, decimal unitPrice)
    {
        var existingItem = _items.FirstOrDefault(x => x.ProductVariantId == productVariantId);
        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
            existingItem.RefreshUnitPrice(unitPrice);
            return;
        }

        _items.Add(CartItem.Create(productVariantId, quantity, unitPrice));
    }

    public void UpdateItemQuantity(int cartItemId, int quantity)
    {
        if (quantity <= 0)
        {
            RemoveItem(cartItemId);
            return;
        }

        var item =
            _items.FirstOrDefault(x => x.Id == cartItemId)
            ?? throw new InvalidOperationException("Cart item not found.");

        item.UpdateQuantity(quantity);
    }

    public void RemoveItem(int cartItemId)
    {
        var item = _items.FirstOrDefault(x => x.Id == cartItemId);
        if (item is not null)
            _items.Remove(item);
    }

    public void Clear()
    {
        _items.Clear();
    }

    public void AssignCustomer(int customerId)
    {
        if (customerId <= 0)
            throw new ArgumentException("Customer id is required.", nameof(customerId));

        CustomerId = customerId;
    }
}
