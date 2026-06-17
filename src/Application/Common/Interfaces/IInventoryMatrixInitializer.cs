public interface IInventoryMatrixInitializer
{
    Task EnsureForProductVariantsAsync(
        IReadOnlyCollection<int> productVariantIds,
        CancellationToken cancellationToken
    );

    Task EnsureForBranchAsync(int branchId, CancellationToken cancellationToken);
}
