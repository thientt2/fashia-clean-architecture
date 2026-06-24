using Fashia.Domain.Constants;

namespace Fashia.Infrastructure.Identity;

internal static class PermissionPolicyDefinitions
{
    public static readonly IReadOnlyCollection<PermissionPolicyDefinition> All =
    [
        // Categories
        new(Policies.ManageCategories, Permissions.Categories.Manage),
        new(Policies.ViewCategories, Permissions.Categories.View, Permissions.Categories.Manage),
        new(
            Policies.CreateCategories,
            Permissions.Categories.Create,
            Permissions.Categories.Manage
        ),
        new(
            Policies.UpdateCategories,
            Permissions.Categories.Update,
            Permissions.Categories.Manage
        ),
        new(
            Policies.DeleteCategories,
            Permissions.Categories.Delete,
            Permissions.Categories.Manage
        ),
        new(
            Policies.ChangeCategoryStatus,
            Permissions.Categories.ChangeStatus,
            Permissions.Categories.Manage
        ),
        // Products
        new(Policies.ManageProducts, Permissions.Products.Manage),
        new(Policies.ViewProducts, Permissions.Products.View, Permissions.Products.Manage),
        new(Policies.CreateProducts, Permissions.Products.Create, Permissions.Products.Manage),
        new(Policies.UpdateProducts, Permissions.Products.Update, Permissions.Products.Manage),
        new(Policies.DeleteProducts, Permissions.Products.Delete, Permissions.Products.Manage),
        new(
            Policies.ChangeProductStatus,
            Permissions.Products.ChangeStatus,
            Permissions.Products.Manage
        ),
        new(Policies.ImportProducts, Permissions.Products.Import, Permissions.Products.Manage),
        // Branch inventories
        new(Policies.ManageBranchInventories, Permissions.BranchInventories.Manage),
        new(
            Policies.ViewBranchInventories,
            Permissions.BranchInventories.View,
            Permissions.BranchInventories.Manage
        ),
        new(
            Policies.AdjustBranchInventories,
            Permissions.BranchInventories.Adjust,
            Permissions.BranchInventories.Manage
        ),
        // Vouchers
        new(Policies.ManageVouchers, Permissions.Vouchers.Manage),
        new(Policies.ViewVouchers, Permissions.Vouchers.View, Permissions.Vouchers.Manage),
        new(Policies.CreateVouchers, Permissions.Vouchers.Create, Permissions.Vouchers.Manage),
        new(Policies.UpdateVouchers, Permissions.Vouchers.Update, Permissions.Vouchers.Manage),
        new(
            Policies.ChangeVoucherStatus,
            Permissions.Vouchers.ChangeStatus,
            Permissions.Vouchers.Manage
        ),
        new(Policies.DeleteVouchers, Permissions.Vouchers.Delete, Permissions.Vouchers.Manage),
    ];
}
