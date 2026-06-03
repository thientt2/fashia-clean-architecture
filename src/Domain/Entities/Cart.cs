namespace Fashia.Domain.Entities;

public class Cart : BaseAuditableEntity
{
    private readonly List<CartItem> _items = new();

    public int? CustomerId { get; private set; }
    public Customer? Customer { get; private set; }

    public CartStatus Status { get; private set; }

    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    private Cart()
    {
        // EF Core
    }

    public Cart(int customerId)
    {
        SetCustomer(customerId);
        Status = CartStatus.Active;
    }

    public void AddItem(int productVariantId, int quantity)
    {
        EnsureActive();

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
        EnsureActive();

        var item =
            _items.FirstOrDefault(x => x.ProductVariantId == productVariantId)
            ?? throw new InvalidOperationException("Cart item not found.");

        item.UpdateQuantity(quantity);
    }

    public void RemoveItem(int productVariantId)
    {
        EnsureActive();

        var item = _items.FirstOrDefault(x => x.ProductVariantId == productVariantId);
        if (item is not null)
            _items.Remove(item);
    }

    public void Clear()
    {
        EnsureActive();
        _items.Clear();
    }

    public void Checkout()
    {
        EnsureActive();
        Status = CartStatus.CheckedOut;
    }

    public void Abandon()
    {
        EnsureActive();
        Status = CartStatus.Abandoned;
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

    private void EnsureActive()
    {
        if (Status != CartStatus.Active)
            throw new InvalidOperationException("Cart is not active.");
    }
}
