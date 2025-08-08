using static System.Net.Mime.MediaTypeNames;

namespace ElasticJobPortal.Models
{
    public class JobDetailsViewModel
    {
        public Job Job { get; set; }
        public List<JobApplication> Applications { get; set; }
        public List<ResumeScore> ResumeScores { get; set; }
    }
}
