namespace ElasticJobPortal.Models
{
    public class ResumeScore
    {
        public int Id { get; set; }
        public int ResumeId { get; set; }
        public int JobId { get; set; }
        public int Score { get; set; }
        public virtual Resume Resume { get; set; }
        public virtual Job Job { get; set; }
    }
}
