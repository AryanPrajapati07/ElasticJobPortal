using Nest;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElasticJobPortal.Models
{
    public class Job
    {
        public int Id { get; set; }
        public string Title { get; set; }
        
        [NotMapped]  //  for suggest indexing
        public CompletionField TitleSuggest { get; set; }
        public string Company { get; set; }
        public string Description { get; set; }

        public string JobType { get; set; } // Full-time, Part-time
        public List<string> Skills { get; set; }
    }
}
