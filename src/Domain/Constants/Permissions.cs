namespace Fashia.Domain.Constants;

public static class Permissions
{
    public static class Categories
    {
        public const string View = "categories.view";
        public const string Create = "categories.create";
        public const string Update = "categories.update";
        public const string ChangeStatus = "categories.change-status";
        public const string Delete = "categories.delete";
        public const string Manage = "categories.manage";
    }

    public static class Products
    {
        public const string View = "products.view";
        public const string Create = "products.create";
        public const string Update = "products.update";
        public const string ChangeStatus = "products.change-status";
        public const string Delete = "products.delete";
        public const string Import = "products.import";
        public const string Manage = "products.manage";
    }

    public static class Branches
    {
        public const string View = "branches.view";
        public const string Create = "branches.create";
        public const string Update = "branches.update";
        public const string Delete = "branches.delete";
        public const string Manage = "branches.manage";
    }

    public static class BranchInventories
    {
        public const string View = "branch-inventories.view";
        public const string Adjust = "branch-inventories.adjust";
        public const string Manage = "branch-inventories.manage";
    }

    public static class Vouchers
    {
        public const string View = "vouchers.view";
        public const string Create = "vouchers.create";
        public const string Update = "vouchers.update";
        public const string ChangeStatus = "vouchers.change-status";
        public const string Delete = "vouchers.delete";
        public const string Manage = "vouchers.manage";
    }
}
