using Fashia.Application.Common.Interfaces;
using Fashia.Domain.Constants;

namespace Fashia.Application.Common.Security;

public class BranchAuthorizationService : IBranchAuthorizationService
{
    private readonly IIdentityService _identityService;

    public BranchAuthorizationService(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<bool> CanManageBranchAsync(
        string userId,
        int branchId,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(userId))
            return false;

        if (await _identityService.IsInRoleAsync(userId, Roles.Administrator))
            return true;

        if (!await _identityService.IsInRoleAsync(userId, Roles.BranchManager))
            return false;

        var userBranchId = await _identityService.GetUserBranchIdAsync(userId);

        return userBranchId == branchId;
    }
}
