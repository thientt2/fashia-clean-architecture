namespace Fashia.Domain.Entities;

public class CustomerAddress : BaseAuditableEntity
{
    public int CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;

    public string CustomerName { get; private set; } = string.Empty;

    public PhoneNumber CustomerPhone { get; private set; } = null!;

    public Address Address { get; private set; } = null!;

    private CustomerAddress()
    {
        // EF Core
    }

    public CustomerAddress(int customerId, Address address)
    {
        if (customerId <= 0)
            throw new ArgumentException("Customer id is required.", nameof(customerId));

        CustomerId = customerId;
        Address = address ?? throw new ArgumentNullException(nameof(address));
    }

    public string ToShippingAddress()
    {
        return string.Join(
            ", ",
            new[] { Address.Line1, Address.Ward, Address.District, Address.Province }.Where(x =>
                !string.IsNullOrWhiteSpace(x)
            )
        );
    }
}
