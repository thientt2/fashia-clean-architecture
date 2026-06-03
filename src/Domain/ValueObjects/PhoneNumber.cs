using System.Text.RegularExpressions;
using Fashia.Domain.Common;

namespace Fashia.Domain.ValueObjects;

public class PhoneNumber : ValueObject
{
    private static readonly Regex PhoneRegex = new(@"^(0|\+84)[0-9]{9}$", RegexOptions.Compiled);

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public string Value { get; private set; }

    public static PhoneNumber Create(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone number is required.");

        phone = phone.Trim();

        if (!PhoneRegex.IsMatch(phone))
            throw new ArgumentException("Invalid phone number format.");

        return new PhoneNumber(phone);
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(PhoneNumber phone)
    {
        return phone.Value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
