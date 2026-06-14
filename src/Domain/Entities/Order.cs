using Fashia.Domain.Builders;
using Fashia.Domain.Enums;
using Fashia.Domain.ValueObjects;

namespace Fashia.Domain.Entities;

public class Order : BaseAuditableEntity
{
    private readonly List<OrderItem> _items = new();

    public int? CustomerId { get; private set; }
    public Customer? Customer { get; private set; }

    public int BranchId { get; private set; }
    public Branch Branch { get; private set; } = null!;

    public string CustomerName { get; private set; } = string.Empty;

    public EmailVO CustomerEmail { get; private set; } = null!;

    public PhoneNumber CustomerPhone { get; private set; } = null!;

    public Address ShippingAddress { get; private set; } = default!;

    public Money SubTotalAmount { get; private set; } = Money.Zero();

    public Money DiscountAmount { get; private set; } = Money.Zero();

    public Money ShippingFee { get; private set; } = Money.Zero();

    public Money TotalAmount { get; private set; } = Money.Zero();

    public PaymentMethod PaymentMethod { get; private set; }

    public string? Note { get; private set; }

    public OrderStatus Status { get; private set; }

    public OrderVoucher? OrderVoucher { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order()
    {
        // EF Core
    }

    private Order(
        int? customerId,
        int branchId,
        string customerName,
        EmailVO customerEmail,
        PhoneNumber customerPhone,
        Address shippingAddress,
        PaymentMethod paymentMethod = PaymentMethod.CashOnDelivery,
        string? note = null
    )
    {
        SetCustomerId(customerId);
        SetBranchId(branchId);
        SetCustomerSnapshot(customerName, customerEmail, customerPhone);
        SetShippingAddress(shippingAddress);
        SetPaymentMethod(paymentMethod);
        SetNote(note);

        SubTotalAmount = Money.Zero();
        DiscountAmount = Money.Zero();
        TotalAmount = Money.Zero();
        Status = OrderStatus.Pending;
    }

    public static OrderBuilder CreateBuilder()
    {
        return new OrderBuilder();
    }

    internal static Order CreateDraft(
        int? customerId,
        int branchId,
        string customerName,
        EmailVO customerEmail,
        PhoneNumber customerPhone,
        Address shippingAddress,
        PaymentMethod paymentMethod,
        string? note
    )
    {
        return new Order(
            customerId,
            branchId,
            customerName,
            customerEmail,
            customerPhone,
            shippingAddress,
            paymentMethod,
            note
        );
    }

    public void AddItem(int productVariantId, Money unitPrice, int quantity)
    {
        EnsureCanModify();

        var existingItem = _items.FirstOrDefault(x => x.ProductVariantId == productVariantId);

        if (existingItem is not null)
        {
            existingItem.IncreaseQuantity(quantity);
            RecalculateTotal();
            return;
        }

        _items.Add(OrderItem.Create(productVariantId, unitPrice, quantity));

        RecalculateTotal();
    }

    public void ApplyVoucher(int voucherId, string voucherCode, Money discountAmount)
    {
        EnsureCanModify();

        if (!_items.Any())
            throw new InvalidOperationException("Cannot apply voucher to an empty order.");

        if (OrderVoucher is not null)
            throw new InvalidOperationException("Order already has a voucher applied.");

        DiscountAmount = discountAmount.CapAt(SubTotalAmount);

        OrderVoucher = new OrderVoucher(voucherId, voucherCode, DiscountAmount);

        RecalculateTotal();
    }

    public void SetShippingFee(Money shippingFee)
    {
        EnsureCanModify();

        ShippingFee = shippingFee ?? throw new ArgumentNullException(nameof(shippingFee));

        RecalculateTotal();
    }

    public void Confirm()
    {
        EnsureStatus(OrderStatus.Pending);

        if (!_items.Any())
            throw new InvalidOperationException("Cannot confirm an empty order.");

        Status = OrderStatus.Confirmed;
    }

    private void RecalculateTotal()
    {
        var subtotal = Money.Zero();

        foreach (var item in _items)
        {
            subtotal = subtotal.Add(item.LineTotal);
        }

        SubTotalAmount = subtotal;
        DiscountAmount = DiscountAmount.CapAt(SubTotalAmount);

        TotalAmount = SubTotalAmount.ApplyDiscount(DiscountAmount).Add(ShippingFee);
    }

    private void EnsureCanModify()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be modified.");
    }

    private void EnsureStatus(params OrderStatus[] allowedStatuses)
    {
        if (!allowedStatuses.Contains(Status))
            throw new InvalidOperationException(
                $"Order status '{Status}' is not allowed for this operation."
            );
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
        EmailVO customerEmail,
        PhoneNumber customerPhone
    )
    {
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name is required.", nameof(customerName));

        customerName = customerName.Trim();

        if (customerName.Length > 200)
            throw new ArgumentException(
                "Customer name must not exceed 200 characters.",
                nameof(customerName)
            );

        if (customerEmail.IsEmpty())
            throw new ArgumentException("Customer email is required.", nameof(customerEmail));

        if (customerPhone.IsEmpty())
            throw new ArgumentException("Customer phone is required.", nameof(customerPhone));

        CustomerName = customerName;
        CustomerEmail = customerEmail;

        CustomerPhone = customerPhone;
    }

    private void SetShippingAddress(Address shippingAddress)
    {
        ShippingAddress =
            shippingAddress ?? throw new ArgumentNullException(nameof(shippingAddress));
    }

    private void SetPaymentMethod(PaymentMethod paymentMethod)
    {
        if (!Enum.IsDefined(typeof(PaymentMethod), paymentMethod))
            throw new ArgumentException("Invalid payment method.", nameof(paymentMethod));

        PaymentMethod = paymentMethod;
    }

    private void SetNote(string? note)
    {
        var value = note?.Trim();

        Note = string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
