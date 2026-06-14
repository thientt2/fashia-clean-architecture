namespace Fashia.Application.Common.Interfaces;

public interface IBranchAuthorizationService
{
    Task<bool> CanManageBranchAsync(
        string userId,
        int branchId,
        CancellationToken cancellationToken = default);
}