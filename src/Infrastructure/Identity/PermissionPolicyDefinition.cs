namespace Fashia.Infrastructure.Identity;

internal sealed record PermissionPolicyDefinition(
    string Policy,
    string Permission,
    string? ManagePermission = null
);
