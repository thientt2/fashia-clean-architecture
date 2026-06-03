using Fashia.Domain.Common;

namespace Fashia.Domain.Entities;

public class Branch : BaseAuditableEntity
{
    public string Name { get; private set; } = string.Empty;
    public PhoneNumber Phone { get; private set; } = null!;
    public EmailVO Email { get; private set; } = null!;
    public Address BranchAddress { get; private set; } = null!;
    public bool IsMain { get; private set; }
    public GeoLocation Location { get; private set; } = null!;
    public BranchStatus Status { get; private set; }

    private Branch()
    {
        // EF Core
    }

    public Branch(
        string name,
        PhoneNumber phone,
        EmailVO email,
        Address address,
        GeoLocation location
    )
    {
        Name = name;
        Phone = phone;
        Email = email;
        BranchAddress = address;
        Status = BranchStatus.Active;
        Location = location;
        IsMain = false;
    }

    public void UpdateName(string name)
    {
        SetName(name);
    }

    public void UpdateAddress(Address address)
    {
        BranchAddress = address;
    }

    public void UpdateIsMain(bool isMain)
    {
        SetIsMain(isMain);
    }

    public void ChangeStatus()
    {
        ToggleStatus();
    }

    public void UpdateContactInfo(PhoneNumber? phone = null, EmailVO? email = null)
    {
        Phone = phone ?? Phone;
        Email = email ?? Email;
    }

    public void UpdateLocation(decimal latitude, decimal longitude)
    {
        Location = GeoLocation.Create(latitude, longitude);
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Branch name cannot be empty.", nameof(name));

        Name = name;
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
}
