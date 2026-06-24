using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Fashia.Infrastructure.Identity;

internal static class AuthorizationPolicyRegistration
{
    public static IServiceCollection AddPermissionAuthorizationPolicies(
        this IServiceCollection services
    )
    {
        var authorizationBuilder = services.AddAuthorizationBuilder();

        foreach (var definition in PermissionPolicyDefinitions.All)
        {
            authorizationBuilder.AddPolicy(
                definition.Policy,
                policy =>
                    policy.RequirePermission(definition.Permission, definition.ManagePermission)
            );
        }

        return services;
    }
}
