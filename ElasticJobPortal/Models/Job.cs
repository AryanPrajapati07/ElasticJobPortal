using Nest;

namespace ElasticJobPortal.Models
{
    public class Job
    {
        public int Id { get; set; }
        public string Title { get; set; }
        
        [Completion]  //  for suggest indexing
        public CompletionField TitleSuggest { get; set; }
        public string Company { get; set; }
        public string Description { get; set; }
    }
}
