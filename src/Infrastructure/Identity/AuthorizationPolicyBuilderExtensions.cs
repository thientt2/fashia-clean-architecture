using Fashia.Domain.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Fashia.Infrastructure.Identity;

public static class AuthorizationPolicyBuilderExtensions
{
    public static AuthorizationPolicyBuilder RequirePermission(
        this AuthorizationPolicyBuilder policy,
        string permission,
        string? managePermission = null
    )
    {
        return policy.RequireAssertion(context =>
        {
            var hasPermission = context.User.HasClaim(CustomClaimTypes.Permission, permission);

            var hasManagePermission =
                managePermission is not null
                && context.User.HasClaim(CustomClaimTypes.Permission, managePermission);

            return hasPermission || hasManagePermission;
        });
    }
}
