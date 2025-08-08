using ElasticJobPortal.Models;
using ElasticJobPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElasticJobPortal.Controllers
{
    [Authorize]
    public class ResumeScoringController : Controller
    {
        private readonly IConfiguration _configuration;  
        private readonly IResumeScorer _resumeScorer;
        private readonly ApplicationDbContext _context;

        public ResumeScoringController(IConfiguration configuration, IResumeScorer resumeScorer, ApplicationDbContext context)
        {
            _configuration = configuration;
            _resumeScorer = resumeScorer;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> ScoreResume(int resumeId, int jobId)
        {
            var score = await _resumeScorer.ScoreResumeAsync(resumeId, jobId);
            return Json(new { ok = true, score });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ScoreAllForJob(int jobId) 
        { 
            var resumes = _context.Set<Resume>().ToList();
            var results = new List<object>();
            foreach (var resume in resumes)
            {
                var score = await _resumeScorer.ScoreResumeAsync(resume.Id, jobId);
                results.Add(new { resumeId = resume.Id, score });
            }
            return Json(new { ok = true, results });
        }


    }
    
}
