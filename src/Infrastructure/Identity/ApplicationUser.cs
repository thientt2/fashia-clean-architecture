using Microsoft.AspNetCore.Identity;

namespace Fashia.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public int? BranchId { get; set; }
}
