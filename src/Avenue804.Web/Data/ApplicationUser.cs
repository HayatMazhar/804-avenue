using Microsoft.AspNetCore.Identity;

namespace Avenue804.Web.Data;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
}
