namespace Fashia.Domain.Entities;

public class Customer : BaseAuditableEntity
{
    public string? UserId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public EmailVO CustomerEmail { get; private set; } = null!;
    public PhoneNumber CustomerPhone { get; private set; } = null!;
    public int Points { get; private set; }
    public CustomerTier Tier { get; private set; }

    public Customer()
    {
        // EF Core
    }

    private Customer(
        string? userId,
        string firstName,
        string lastName,
        EmailVO customerEmail,
        PhoneNumber customerPhone
    )
    {
        if (userId is not null && string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User id cannot be empty if provided.", nameof(userId));
        if (customerEmail.IsEmpty())
            throw new ArgumentException("Customer email is required.", nameof(customerEmail));
        if (customerPhone.IsEmpty())
            throw new ArgumentException("Customer phone is required.", nameof(customerPhone));

        UserId = userId;
        SetName(firstName, lastName);
        CustomerEmail = customerEmail;
        CustomerPhone = customerPhone;

        Points = 0;
        Tier = CustomerTier.Bronze;
    }

    public static Customer Create(
        string? userId,
        string firstName,
        string lastName,
        EmailVO customerEmail,
        PhoneNumber customerPhone
    )
    {
        return new Customer(userId, firstName, lastName, customerEmail, customerPhone);
    }

    public void AddPoints(int points)
    {
        if (points < 0)
        {
            throw new ArgumentException("Points to add cannot be negative.", nameof(points));
        }
        Points += points;
        UpdateTier();
    }

    private void UpdateTier()
    {
        Tier = Points switch
        {
            >= 10000 => CustomerTier.Platinum,
            >= 5000 => CustomerTier.Gold,
            >= 1000 => CustomerTier.Silver,
            _ => CustomerTier.Bronze,
        };
    }

    private void SetName(string firstName, string lastName)
    {
        if (string.IsNullOrEmpty(firstName))
        {
            throw new ArgumentException("First name cannot be null or empty.", nameof(firstName));
        }
        if (string.IsNullOrEmpty(lastName))
        {
            throw new ArgumentException("Last name cannot be null or empty.", nameof(lastName));
        }
        FirstName = firstName;
        LastName = lastName;
    }
}
