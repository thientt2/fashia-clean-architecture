namespace Fashia.Domain.Entities;

public class Order : BaseAuditableEntity
{
    private readonly List<OrderItem> _items = new();
    private readonly List<OrderVoucher> _vouchers = new();

    public int? CustomerId { get; private set; }
    public Customer? Customer { get; private set; }

    public int BranchId { get; private set; }
    public Branch Branch { get; private set; } = null!;

    public string CustomerName { get; private set; } = string.Empty;
    public string? CustomerEmail { get; private set; }
    public string CustomerPhone { get; private set; } = string.Empty;
    public string ShippingAddress { get; private set; } = string.Empty;

    public decimal SubTotalAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<OrderVoucher> Vouchers => _vouchers.AsReadOnly();

    private Order()
    {
        // EF Core
    }

    public Order(int customerId, int branchId)
        : this(customerId, branchId, "Customer", null, "N/A", "N/A")
    {
    }

    public Order(
        int? customerId,
        int branchId,
        string customerName,
        string? customerEmail,
        string customerPhone,
        string shippingAddress
    )
    {
        SetCustomerId(customerId);
        SetBranchId(branchId);
        SetCustomerSnapshot(customerName, customerEmail, customerPhone, shippingAddress);
        SubTotalAmount = 0;
        DiscountAmount = 0;
        TotalAmount = 0;
        Status = OrderStatus.Pending;
    }

    public void AddItem(int productVariantId, int quantity, decimal unitPrice)
    {
        var existingItem = _items.FirstOrDefault(x => x.ProductVariantId == productVariantId);

        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
            RecalculateTotal();
            return;
        }

        _items.Add(new OrderItem(productVariantId, quantity, unitPrice));

        RecalculateTotal();
    }

    public void UpdateStatus(OrderStatus newStatus)
    {
        if (!Enum.IsDefined(typeof(OrderStatus), newStatus))
            throw new ArgumentException("Invalid order status.", nameof(newStatus));

        Status = newStatus;
    }

    public void ApplyVoucher(int voucherId, string voucherCode, decimal discountAmount)
    {
        if (voucherId <= 0)
            throw new ArgumentException("Voucher id is required.", nameof(voucherId));

        if (string.IsNullOrWhiteSpace(voucherCode))
            throw new ArgumentException("Voucher code is required.", nameof(voucherCode));

        if (discountAmount < 0)
            throw new ArgumentException("Discount amount cannot be negative.", nameof(discountAmount));

        if (_vouchers.Count != 0)
            throw new InvalidOperationException("Order already has a voucher applied.");

        DiscountAmount = decimal.Round(discountAmount, 2, MidpointRounding.AwayFromZero);
        _vouchers.Add(new OrderVoucher(voucherId, voucherCode, DiscountAmount));
        RecalculateTotal();
    }

    private void RecalculateTotal()
    {
        SubTotalAmount = _items.Sum(x => x.LineTotal);
        TotalAmount = Math.Max(0, SubTotalAmount - DiscountAmount);
    }

    private void SetCustomerId(int? customerId)
    {
        if (customerId.HasValue && customerId <= 0)
            throw new ArgumentException("Customer id is required.", nameof(customerId));

        CustomerId = customerId;
    }

    private void SetBranchId(int branchId)
    {
        if (branchId <= 0)
            throw new ArgumentException("Branch id is required.", nameof(branchId));

        BranchId = branchId;
    }

    private void SetCustomerSnapshot(
        string customerName,
        string? customerEmail,
        string customerPhone,
        string shippingAddress
    )
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name is required.", nameof(customerName));

        if (customerName.Length > 200)
            throw new ArgumentException("Customer name must not exceed 200 characters.", nameof(customerName));

        if (!string.IsNullOrWhiteSpace(customerEmail) && customerEmail.Length > 256)
            throw new ArgumentException("Customer email must not exceed 256 characters.", nameof(customerEmail));

        if (string.IsNullOrWhiteSpace(customerPhone))
            throw new ArgumentException("Customer phone is required.", nameof(customerPhone));

        if (customerPhone.Length > 20)
            throw new ArgumentException("Customer phone must not exceed 20 characters.", nameof(customerPhone));

        if (string.IsNullOrWhiteSpace(shippingAddress))
            throw new ArgumentException("Shipping address is required.", nameof(shippingAddress));

        if (shippingAddress.Length > 500)
            throw new ArgumentException("Shipping address must not exceed 500 characters.", nameof(shippingAddress));

        CustomerName = customerName.Trim();
        CustomerEmail = string.IsNullOrWhiteSpace(customerEmail) ? null : customerEmail.Trim();
        CustomerPhone = customerPhone.Trim();
        ShippingAddress = shippingAddress.Trim();
    }
}
