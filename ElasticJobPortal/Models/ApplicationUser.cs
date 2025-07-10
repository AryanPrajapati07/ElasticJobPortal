using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    // Extra fields (optional)
    public string? FullName { get; set; }
}
