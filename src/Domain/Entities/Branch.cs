using Fashia.Domain.Common;

namespace Fashia.Domain.Entities;

public class Branch : BaseAuditableEntity
{
    private Branch()
    {
        // EF Core
    }

    public Branch(
        string name,
        string address,
        string phone,
        bool isMain,
        decimal latitude,
        decimal longitude
    )
    {
        SetName(name);
        SetAddress(address);
        SetPhone(phone);
        SetIsMain(isMain);
        SetLocation(latitude, longitude);
    }

    public string Name { get; private set; } = string.Empty;

    public string Address { get; private set; } = null!;

    public string Phone { get; private set; } = null!;

    public bool IsMain { get; private set; }

    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }
    public BranchStatus Status { get; private set; } = BranchStatus.Active;

    public void Rename(string name)
    {
        SetName(name);
    }

    public void UpdateAddress(string address)
    {
        SetAddress(address);
    }

    public void UpdatePhone(string phone)
    {
        SetPhone(phone);
    }

    public void UpdateIsMain(bool isMain)
    {
        SetIsMain(isMain);
    }

    public void ChangeStatus()
    {
        ToggleStatus();
    }

    public void UpdateLocation(decimal latitude, decimal longitude)
    {
        SetLocation(latitude, longitude);
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Branch name must not be empty.", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException(
                "Branch name must not exceed 100 characters.",
                nameof(name)
            );

        Name = name;
    }

    private void SetAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Branch address must not be empty.", nameof(address));

        if (address.Length > 250)
            throw new ArgumentException(
                "Branch address must not exceed 250 characters.",
                nameof(address)
            );

        Address = address;
    }

    private void SetPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Branch phone must not be empty.", nameof(phone));

        if (phone.Length > 20)
            throw new ArgumentException(
                "Branch phone must not exceed 20 characters.",
                nameof(phone)
            );

        Phone = phone;
    }

    private void SetIsMain(bool isMain)
    {
        if (isMain && Status == BranchStatus.Inactive)
            throw new InvalidOperationException("Inactive branch cannot be set as main.");

        IsMain = isMain;
    }

    private void ToggleStatus()
    {
        if (IsMain && Status == BranchStatus.Inactive)
            throw new InvalidOperationException("Inactive branch cannot be set as main.");

        Status = Status == BranchStatus.Active ? BranchStatus.Inactive : BranchStatus.Active;
    }

    private void SetLocation(decimal latitude, decimal longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new ArgumentOutOfRangeException(
                nameof(latitude),
                "Latitude must be between -90 and 90."
            );
        if (longitude < -180 || longitude > 180)
            throw new ArgumentOutOfRangeException(
                nameof(longitude),
                "Longitude must be between -180 and 180."
            );
        Latitude = latitude;
        Longitude = longitude;
    }
}
