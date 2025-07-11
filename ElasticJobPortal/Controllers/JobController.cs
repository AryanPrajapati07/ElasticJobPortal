using ElasticJobPortal.Elastic;
using ElasticJobPortal.Models;
using ElasticJobPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nest;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ElasticJobPortal.Controllers
{
    [Authorize(Roles = "Admin")]

    public class JobController : Controller
    {
        private readonly JobSearchService _searchService = new();
        private readonly JobIndexerService _indexer = new();
        private readonly ElasticClient _elasticClient;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _usermanager;


        public JobController(ElasticClient elasticClient,ApplicationDbContext context)
        {
           
            _elasticClient = elasticClient;
            _context = context;
        }

        //Job Post
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Models.Job job)
        {
            if (!ModelState.IsValid)
            {
                return View(job);
            }

            // Convert CSV to List
            job.Skills = job.SkillsCsv?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                       .Select(s => s.Trim())
                                       .ToList() ?? new List<string>();

            // Save to SQL DB
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();
            // Set TitleSuggest before sending to Elastic
            job.TitleSuggest = new CompletionField
            {
                Input = new[] { job.Title }
            };

            // Elastic indexing
            var indexResponse = await _elasticClient.IndexDocumentAsync(job);

            // LOGGING
            Console.WriteLine("📦 Elastic IsValid: " + indexResponse.IsValid);
            Console.WriteLine("📨 Elastic Response: " + indexResponse.DebugInformation);

            if (!indexResponse.IsValid)
            {
                TempData["ErrorMessage"] = "Elastic Error: " + indexResponse.ServerError?.ToString();
            }


            return RedirectToAction("Index");
        }





        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> Apply(int jobId)
        {
            var userId = _usermanager.GetUserId(User);
            var alreadyApplied = await _context.JobApplications
                .AnyAsync(ja => ja.JobId == jobId.ToString() && ja.UserId == userId); // Convert jobId to string

            if (!alreadyApplied)
            {
                _context.JobApplications.Add(new JobApplication
                {
                    JobId = jobId.ToString(), // Convert jobId to string
                    UserId = userId,
                    AppliedOn = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Application submitted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "You have already applied for this job.";
            }
            return RedirectToAction("Search");
        }

        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> MyApplications()
        {
            var userId = _usermanager.GetUserId(User);
            var applications = await _context.JobApplications
                .Where(ja => ja.UserId == userId)
                .ToListAsync();
            return View(applications);
        }




        // Index sample data
        public IActionResult Seed()
        {
            _indexer.IndexSampleJobs();
            return Content("3 Jobs inserted into ElasticSearch.");
        }


        // Search jobs
        public IActionResult Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return View(new List<Models.Job>());


            var results = _searchService.SearchJobs(keyword);
            return View(results);
        }

        // Optional: Show all jobs (dummy)
        public IActionResult Index()
        {
            return RedirectToAction("Search");
        }

        [HttpGet]
        public async Task<JsonResult> GetSuggestions(string term)
        {
            try
            {
                var response = await _elasticClient.SearchAsync<Models.Job>(s => s
                    .Suggest(su => su
                        .Completion("job-suggestions", cs => cs
                            .Field("titleSuggest") // <- use string field name
                            .Prefix(term)
                            .Fuzzy(f => f.Fuzziness(Fuzziness.Auto))
                            .Size(5)
                        )
                    )
                );

                // Debug log
                Console.WriteLine("ES DEBUG: " + response.DebugInformation);

                if (!response.IsValid)
                {
                    return Json(new List<string> { "Error: Invalid response from ES" });
                }

                var suggestions = response.Suggest["job-suggestions"]
                    .SelectMany(s => s.Options)
                    .Select(o => o.Text)
                    .Distinct()
                    .ToList();

                return Json(suggestions);
            }
            catch (Exception ex)
            {
                return Json(new List<string> { "Exception: " + ex.Message });
            }
        }

        //[HttpGet]
        //public async Task<IActionResult> JobStats()
        //{
        //    var response = await _elasticClient.SearchAsync<Models.Job>(s => s
        //        .Size(0) // Don't return documents
        //        .Aggregations(a => a
        //            .Terms("titles", t => t.Field(f => f.Title.Suffix("keyword")).Size(10))
        //            .Terms("companies", c => c.Field(f => f.Company.Suffix("keyword")).Size(10))
        //        )
        //    );

        //    var titleBuckets = response.Aggregations.Terms("titles").Buckets;
        //    var companyBuckets = response.Aggregations.Terms("companies").Buckets;

        //    var stats = new
        //    {
        //        Titles = titleBuckets.Select(b => new { Name = b.Key, Count = b.DocCount }),
        //        Companies = companyBuckets.Select(b => new { Name = b.Key, Count = b.DocCount })
        //    };

        //    return Json(stats);
        //}

      



    }
}
