
namespace ElasticJobPortal.Models
{
    public class Resume
    {

        public int Id { get; set; }
        public string JobSeekerId { get; set; }
        public string ContentText { get; set; } //resume content in text format
        public string FileName { get; internal set; }
        public DateTime UploadedAt { get; internal set; }
        public string ResumeKeywords { get; internal set; }
    }
}
