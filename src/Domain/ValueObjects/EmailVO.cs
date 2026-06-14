using System.Net.Mail;
using Fashia.Domain.Common;

namespace Fashia.Domain.ValueObjects;

public sealed class EmailVO : ValueObject
{
    private EmailVO()
    {
        // EF Core
    }

    private EmailVO(string value)
    {
        Value = value;
    }

    public string Value { get; private set; } = string.Empty;

    public static EmailVO Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));

        email = email.Trim();

        try
        {
            var mail = new MailAddress(email);

            if (!string.Equals(mail.Address, email, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Invalid email format.", nameof(email));
        }
        catch
        {
            throw new ArgumentException("Invalid email format.", nameof(email));
        }

        return new EmailVO(email);
    }

    public bool IsEmpty()
    {
        return string.IsNullOrWhiteSpace(Value);
    }

    public static implicit operator string(EmailVO email)
    {
        return email.Value;
    }

    public override string ToString()
    {
        return Value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value.ToLowerInvariant();
    }
}
