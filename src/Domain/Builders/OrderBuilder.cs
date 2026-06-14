using Fashia.Domain.Entities;
using Fashia.Domain.Enums;
using Fashia.Domain.ValueObjects;

namespace Fashia.Domain.Builders;

public sealed class OrderBuilder
{
    private int? _customerId;
    private int? _branchId;

    private string? _customerName;
    private EmailVO? _customerEmail;
    private PhoneNumber? _customerPhone;

    private Address? _shippingAddress;
    private PaymentMethod _paymentMethod = PaymentMethod.CashOnDelivery;
    private string? _note;

    private readonly List<OrderItemBuildInput> _items = new();

    internal OrderBuilder() { }

    public OrderBuilder ForCustomer(
        int customerId,
        string customerName,
        EmailVO customerEmail,
        PhoneNumber customerPhone
    )
    {
        if (customerId <= 0)
            throw new ArgumentException("Customer id is required.", nameof(customerId));

        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name is required.", nameof(customerName));

        if (customerEmail.IsEmpty())
            throw new ArgumentException("Customer email is required.", nameof(customerEmail));

        if (customerPhone.IsEmpty())
            throw new ArgumentException("Customer phone is required.", nameof(customerPhone));

        _customerId = customerId;
        _customerName = customerName.Trim();
        _customerEmail = customerEmail;
        _customerPhone = customerPhone;

        return this;
    }

    public OrderBuilder FromBranch(int branchId)
    {
        if (branchId <= 0)
            throw new ArgumentException("Branch id is required.", nameof(branchId));

        _branchId = branchId;

        return this;
    }

    public OrderBuilder ShipTo(Address shippingAddress)
    {
        _shippingAddress =
            shippingAddress ?? throw new ArgumentNullException(nameof(shippingAddress));

        return this;
    }

    public OrderBuilder PaidBy(PaymentMethod paymentMethod)
    {
        if (!Enum.IsDefined(typeof(PaymentMethod), paymentMethod))
            throw new ArgumentException("Invalid payment method.", nameof(paymentMethod));

        _paymentMethod = paymentMethod;

        return this;
    }

    public OrderBuilder WithNote(string? note)
    {
        _note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();

        return this;
    }

    public OrderBuilder AddItem(int productVariantId, Money unitPrice, int quantity)
    {
        _items.Add(new OrderItemBuildInput(productVariantId, unitPrice, quantity));

        return this;
    }

    public Order Build()
    {
        if (_customerId is null)
            throw new InvalidOperationException("Customer is required.");

        if (_branchId is null)
            throw new InvalidOperationException("Branch is required.");

        if (_customerName is null)
            throw new InvalidOperationException("Customer name is required.");

        if (_customerEmail is null || _customerEmail.IsEmpty())
            throw new InvalidOperationException("Customer email is required.");

        if (_customerPhone is null || _customerPhone.IsEmpty())
            throw new InvalidOperationException("Customer phone is required.");

        if (_shippingAddress is null)
            throw new InvalidOperationException("Shipping address is required.");

        if (_items.Count == 0)
            throw new InvalidOperationException("Cannot build order without items.");

        var order = Order.CreateDraft(
            _customerId.Value,
            _branchId.Value,
            _customerName,
            _customerEmail,
            _customerPhone,
            _shippingAddress.Copy(),
            _paymentMethod,
            _note
        );

        foreach (var item in _items)
        {
            order.AddItem(item.ProductVariantId, item.UnitPrice, item.Quantity);
        }

        return order;
    }

    private sealed record OrderItemBuildInput(int ProductVariantId, Money UnitPrice, int Quantity);
}
