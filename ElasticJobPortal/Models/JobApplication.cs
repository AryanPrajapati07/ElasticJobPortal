namespace ElasticJobPortal.Models
{
    public class JobApplication
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int JobId { get; set; }
        public DateTime AppliedOn { get; set; }
        public string Status { get; set; } = "Pending";
        public string ResumePath { get; set; }
        public ApplicationUser User { get; set; }
        public Job Job { get; set; }
    }


}
