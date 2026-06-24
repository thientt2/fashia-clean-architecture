namespace Fashia.Domain.Entities;

public class Voucher : BaseAuditableEntity
{
    public string Code { get; private set; } = string.Empty;

    public DiscountType DiscountType { get; private set; }

    public int DiscountAmount { get; private set; }

    public int MinOrderAmount { get; private set; }

    public int MaxDiscountAmount { get; private set; }

    public int UsageLimit { get; private set; }

    public int UsedCount { get; private set; }

    public DateTime ValidFrom { get; private set; }

    public DateTime ValidUntil { get; private set; }

    public VoucherType VoucherType { get; private set; }

    public int? ProductId { get; private set; }
    public Product? Product { get; private set; }

    public int? CategoryId { get; private set; }
    public Category? Category { get; private set; }

    public int? BrandId { get; private set; }
    public Brand? Brand { get; private set; }

    public VoucherStatus Status { get; private set; }

    public Display Display { get; private set; }

    public int QuantityPerUser { get; private set; }

    public uint Version { get; private set; }

    private Voucher()
    {
        // EF Core
    }

    public Voucher(
        string code,
        int discountAmount,
        DateTime validFrom,
        DateTime validUntil,
        VoucherType voucherType,
        DiscountType discountType = DiscountType.FixedAmount,
        Display display = Display.Public,
        int quantityPerUser = 1,
        int usageLimit = 0,
        int minOrderAmount = 0,
        int maxDiscountAmount = 0,
        int? productId = null,
        int? categoryId = null,
        int? brandId = null
    )
    {
        SetCode(code);
        SetDiscountType(discountType);
        SetDateRange(validFrom, validUntil);
        SetUsageLimit(usageLimit);
        SetMinOrderAmount(minOrderAmount);
        SetMaxDiscountAmount(maxDiscountAmount);
        SetDiscount(discountType, discountAmount);
        SetVoucherType(voucherType);
        SetTargets(productId, categoryId, brandId);
        SetDisplay(display);
        SetQuantityPerUser(quantityPerUser);
        Status = VoucherStatus.Active;
    }

    public void UpdateCode(string code)
    {
        SetCode(code);
    }

    public void UpdateDiscount(DiscountType discountType, int discountAmount)
    {
        SetDiscount(discountType, discountAmount);
    }

    public void UpdateMinOrderAmount(int minOrderAmount)
    {
        SetMinOrderAmount(minOrderAmount);
    }

    public void UpdateMaxDiscountAmount(int maxDiscountAmount)
    {
        SetMaxDiscountAmount(maxDiscountAmount);
    }

    public void UpdateUsageLimit(int usageLimit)
    {
        SetUsageLimit(usageLimit);
    }

    public void UpdateDateRange(DateTime validFrom, DateTime validUntil)
    {
        SetDateRange(validFrom, validUntil);
    }

    public void UpdateVoucherType(VoucherType voucherType)
    {
        UpdateApplicability(voucherType, ProductId, CategoryId, BrandId);
    }

    public void UpdateApplicability(
        VoucherType voucherType,
        int? productId,
        int? categoryId,
        int? brandId
    )
    {
        SetVoucherType(voucherType);
        SetTargets(productId, categoryId, brandId);
    }

    public void UpdateDisplay(Display display)
    {
        SetDisplay(display);
    }

    public void UpdateQuantityPerUser(int quantityPerUser)
    {
        SetQuantityPerUser(quantityPerUser);
    }

    public void Deactivate()
    {
        if (Status == VoucherStatus.Suspended)
            throw new InvalidOperationException("Cannot deactivate a suspended voucher.");

        Status = VoucherStatus.Inactive;
    }

    public void Suspend()
    {
        if (Status != VoucherStatus.Active)
            throw new InvalidOperationException("Only active voucher can be suspended.");

        Status = VoucherStatus.Suspended;
    }

    public void Reactivate()
    {
        if (Status != VoucherStatus.Inactive && Status != VoucherStatus.Suspended)
            throw new InvalidOperationException("Cannot reactivate voucher.");

        if (ValidUntil < DateTime.UtcNow)
            throw new InvalidOperationException("Cannot reactivate expired voucher.");

        Status = VoucherStatus.Active;
    }

    public long CalculateDiscount(long orderAmount)
    {
        if (Status != VoucherStatus.Active)
            throw new InvalidOperationException("Voucher is not active.");

        if (DateTime.UtcNow < ValidFrom || DateTime.UtcNow > ValidUntil)
            throw new InvalidOperationException("Voucher is not valid at this time.");

        if (UsageLimit > 0 && UsedCount >= UsageLimit)
            throw new InvalidOperationException("Voucher usage limit has been reached.");

        if (orderAmount < MinOrderAmount)
            throw new InvalidOperationException("Order amount does not meet voucher minimum.");

        var discount =
            DiscountType == DiscountType.Percentage
                ? orderAmount * DiscountAmount / 100
                : DiscountAmount;

        if (MaxDiscountAmount > 0)
            discount = Math.Min(discount, MaxDiscountAmount);

        return discount;
    }

    public void MarkUsed()
    {
        if (UsageLimit > 0 && UsedCount >= UsageLimit)
            throw new InvalidOperationException("Voucher usage limit has been reached.");

        UsedCount++;
    }

    public bool IsApplicableTo(
        IEnumerable<int> productIds,
        IEnumerable<int> categoryIds,
        IEnumerable<int> brandIds
    )
    {
        ArgumentNullException.ThrowIfNull(productIds);
        ArgumentNullException.ThrowIfNull(categoryIds);
        ArgumentNullException.ThrowIfNull(brandIds);

        return VoucherType switch
        {
            VoucherType.All => true,
            VoucherType.ProductSpecific => ProductId.HasValue
                && productIds.Contains(ProductId.Value),
            VoucherType.CategorySpecific => CategoryId.HasValue
                && categoryIds.Contains(CategoryId.Value),
            VoucherType.BrandSpecific => BrandId.HasValue && brandIds.Contains(BrandId.Value),
            _ => false,
        };
    }

    private void SetCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Voucher code cannot be empty.");

        Code = code.Trim().ToUpperInvariant();
    }

    private void SetDiscountAmount(int discountAmount)
    {
        if (discountAmount <= 0)
            throw new ArgumentException("Discount amount must be greater than zero.");

        if (DiscountType == DiscountType.Percentage && discountAmount > 100)
            throw new ArgumentException("Percentage discount cannot exceed 100.");

        DiscountAmount = discountAmount;
    }

    private void SetDiscountType(DiscountType discountType)
    {
        if (!Enum.IsDefined(typeof(DiscountType), discountType))
            throw new ArgumentException("Invalid discount type.", nameof(discountType));

        DiscountType = discountType;
    }

    private void SetMinOrderAmount(int minOrderAmount)
    {
        if (minOrderAmount < 0)
            throw new ArgumentException("Minimum order amount cannot be negative.");

        MinOrderAmount = minOrderAmount;
    }

    private void SetMaxDiscountAmount(int maxDiscountAmount)
    {
        if (maxDiscountAmount < 0)
            throw new ArgumentException("Maximum discount amount cannot be negative.");

        MaxDiscountAmount = maxDiscountAmount;
    }

    private void SetUsageLimit(int usageLimit)
    {
        if (usageLimit < 0)
            throw new ArgumentException("Usage limit cannot be negative.");

        UsageLimit = usageLimit;
    }

    private void SetDateRange(DateTime validFrom, DateTime validUntil)
    {
        if (validFrom >= validUntil)
            throw new ArgumentException("Valid from date must be earlier than valid until date.");

        ValidFrom = validFrom;
        ValidUntil = validUntil;
    }

    private void SetVoucherType(VoucherType voucherType)
    {
        if (!Enum.IsDefined(typeof(VoucherType), voucherType))
            throw new ArgumentException("Invalid voucher type.", nameof(voucherType));

        VoucherType = voucherType;
    }

    private void SetTargets(int? productId, int? categoryId, int? brandId)
    {
        EnsurePositiveTarget(productId, nameof(productId));
        EnsurePositiveTarget(categoryId, nameof(categoryId));
        EnsurePositiveTarget(brandId, nameof(brandId));

        var valid = VoucherType switch
        {
            VoucherType.All => productId is null && categoryId is null && brandId is null,
            VoucherType.ProductSpecific => productId.HasValue
                && categoryId is null
                && brandId is null,
            VoucherType.CategorySpecific => productId is null
                && categoryId.HasValue
                && brandId is null,
            VoucherType.BrandSpecific => productId is null
                && categoryId is null
                && brandId.HasValue,
            _ => false,
        };

        if (!valid)
            throw new ArgumentException("Voucher target does not match voucher type.");

        ProductId = productId;
        CategoryId = categoryId;
        BrandId = brandId;
    }

    private static void EnsurePositiveTarget(int? targetId, string paramName)
    {
        if (targetId <= 0)
            throw new ArgumentException("Voucher target id must be greater than zero.", paramName);
    }

    private void SetDisplay(Display display)
    {
        if (!Enum.IsDefined(typeof(Display), display))
            throw new ArgumentException("Invalid display value.", nameof(display));

        Display = display;
    }

    private void SetQuantityPerUser(int quantityPerUser)
    {
        if (quantityPerUser < 0)
            throw new ArgumentException("Quantity per user cannot be negative.");

        QuantityPerUser = quantityPerUser;
    }

    private static void ValidateDiscount(DiscountType discountType, int discountAmount)
    {
        if (!Enum.IsDefined(typeof(DiscountType), discountType))
            throw new ArgumentException("Invalid discount type.", nameof(discountType));

        if (discountAmount <= 0)
            throw new ArgumentException(
                "Discount amount must be greater than zero.",
                nameof(discountAmount)
            );

        if (discountType == DiscountType.Percentage && discountAmount > 100)
            throw new ArgumentException(
                "Percentage discount cannot exceed 100.",
                nameof(discountAmount)
            );
    }

    private void SetDiscount(DiscountType discountType, int discountAmount)
    {
        ValidateDiscount(discountType, discountAmount);

        DiscountType = discountType;
        DiscountAmount = discountAmount;
    }
}
