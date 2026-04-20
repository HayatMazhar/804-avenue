using Microsoft.AspNetCore.Identity;

namespace Avenue804.Web.Data;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public bool IsPublicUser { get; set; } = true;
    public bool PhoneVerified { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
