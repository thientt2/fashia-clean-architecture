using Fashia.Application.Common.Models;

namespace Fashia.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<bool> RoleExistsAsync(string role);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);

    Task<Result> DeleteUserAsync(string userId);

    Task<int?> GetUserBranchIdAsync(string userId);
    Task<Result> AddToRoleAsync(string userId, string role);
}
