namespace Fashia.Domain.Entities;

public class Cart : BaseAuditableEntity
{
    private readonly List<CartItem> _items = new();

    public int CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    private Cart()
    {
        // EF Core
    }

    private Cart(Customer customer)
    {
        Customer = customer ?? throw new ArgumentNullException(nameof(customer));
    }

    public static Cart Create(Customer customer)
    {
        return new Cart(customer);
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

    public void AssignCustomer(Customer customer)
    {
        Customer = customer ?? throw new ArgumentNullException(nameof(customer));
    }
}
