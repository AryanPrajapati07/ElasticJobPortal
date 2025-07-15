using ElasticJobPortal.Models;

public class ProfileViewModel
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }

    public IFormFile ResumeFile { get; set; }
    public string? ResumePath { get; set; }

    public List<JobApplication> Applications { get; set; }
    public List<SavedJob> SavedJobs { get; set; }
}
