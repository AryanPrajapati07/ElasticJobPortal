using ElasticJobPortal.Models;

namespace ElasticJobPortal.Services
{
    public class RecommendationService
    {
        private readonly ApplicationDbContext _context;
        public RecommendationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Job> RecommendJobs(string jobSeekerId)
        {
            var resume = _context.Resumes.FirstOrDefault(r => r.JobSeekerId == jobSeekerId);
            if (resume == null || string.IsNullOrEmpty(resume.ContentText))
            {
                return new List<Job>(); // No resume found, return empty list
            }

            var resumeKeywords = ExtractKeywords(resume.ContentText);
            var jobs = _context.Jobs.ToList();

            var matchedJobs = jobs
                .Select(job => new
                {
                    Job = job,
                    MatchScore = resumeKeywords
                                        .Intersect(SplitTags(job.Tags), StringComparer.OrdinalIgnoreCase)
                                        .Count()
                })
                .Where(x => x.MatchScore > 0)
                .OrderByDescending(x => x.MatchScore)
                .Select(x => x.Job)
                .ToList();

            return matchedJobs;

        }

        private List<string> ExtractKeywords(string content)
        {
            // Simple keyword extraction logic
           var keywords = content.ToLower()
                .Split(new[] { ' ', ',', '.', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToList();
            return keywords;
        }

        private List<string> SplitTags(string tags)
        {
            return tags?.ToLower()
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(t => t.Trim())
                        .ToList() ?? new List<string>();
        }


    }
}
