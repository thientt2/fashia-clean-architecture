namespace Fashia.Domain.Entities;

public class Customer : BaseAuditableEntity
{
    public string? UserId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public int Points { get; private set; }
    public CustomerTier Tier { get; private set; }

    public Customer()
    {
        // EF Core
    }

    public Customer(string? userId, string firstName, string lastName)
    {
        SetUserId(userId);
        SetName(firstName, lastName);
        Points = 0;
        Tier = CustomerTier.Bronze;
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

    private void SetUserId(string? userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException("UserId cannot be null or empty.", nameof(userId));
        }
        UserId = userId;
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
