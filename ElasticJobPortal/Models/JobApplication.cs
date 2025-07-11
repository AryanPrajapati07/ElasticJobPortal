namespace ElasticJobPortal.Models
{
    public class JobApplication
    {
        public int Id { get; set; }
        public string JobId { get; set; }
        public string UserId { get; set; }
        public DateTime AppliedOn { get; set; }
    }

}
