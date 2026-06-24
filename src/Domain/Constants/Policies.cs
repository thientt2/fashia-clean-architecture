namespace Fashia.Domain.Constants;

public static class Policies
{
    // Category Policies
    public const string CreateCategories = nameof(CreateCategories);
    public const string UpdateCategories = nameof(UpdateCategories);
    public const string DeleteCategories = nameof(DeleteCategories);
    public const string ViewCategories = nameof(ViewCategories);
    public const string ChangeCategoryStatus = nameof(ChangeCategoryStatus);
    public const string ManageCategories = nameof(ManageCategories);

    // Product Policies
    public const string ViewProducts = nameof(ViewProducts);
    public const string CreateProducts = nameof(CreateProducts);
    public const string UpdateProducts = nameof(UpdateProducts);
    public const string DeleteProducts = nameof(DeleteProducts);
    public const string ChangeProductStatus = nameof(ChangeProductStatus);
    public const string ImportProducts = nameof(ImportProducts);
    public const string ManageProducts = nameof(ManageProducts);

    // Branch Policies
    public const string ManageBranches = nameof(ManageBranches);

    // Branch Inventory Policies
    public const string ViewBranchInventories = nameof(ViewBranchInventories);
    public const string AdjustBranchInventories = nameof(AdjustBranchInventories);
    public const string ManageBranchInventories = nameof(ManageBranchInventories);

    // Order Policies
    public const string ManageOrders = nameof(ManageOrders);

    // Voucher Policies
    public const string ViewVouchers = nameof(ViewVouchers);
    public const string CreateVouchers = nameof(CreateVouchers);
    public const string UpdateVouchers = nameof(UpdateVouchers);
    public const string ChangeVoucherStatus = nameof(ChangeVoucherStatus);
    public const string DeleteVouchers = nameof(DeleteVouchers);
    public const string ManageVouchers = nameof(ManageVouchers);
}
