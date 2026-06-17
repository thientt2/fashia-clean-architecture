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

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Phone number is required.");

        value = value.Trim();

        if (!PhoneRegex.IsMatch(value))
            throw new ArgumentException("Invalid phone number format.");

        return new PhoneNumber(value);
    }

    public static bool IsValid(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return PhoneRegex.IsMatch(value.Trim());
    }

    public bool IsEmpty()
    {
        return string.IsNullOrWhiteSpace(Value);
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
