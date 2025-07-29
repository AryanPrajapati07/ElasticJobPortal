using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    // Extra fields (optional)
    public string FullName { get; set; }
    public string Bio { get; set; }
    public string Location { get; set; }
    public string ResumePath { get; set; }

    public string ResumeKeywords { get; set; } // Comma-separated keywords for resume search

}
