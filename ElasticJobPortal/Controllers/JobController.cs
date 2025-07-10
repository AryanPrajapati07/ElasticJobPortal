using ElasticJobPortal.Elastic;
using ElasticJobPortal.Models;
using Microsoft.AspNetCore.Mvc;
using Nest;
using System.Collections.Generic;

namespace ElasticJobPortal.Controllers
{
    public class JobController : Controller
    {
        private readonly JobSearchService _searchService = new();
        private readonly JobIndexerService _indexer = new();
        private readonly ElasticClient _elasticClient;

        public JobController()
        {
            _elasticClient = ElasticClientProvider.GetClient(); // 👈 This line fixes the error
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

        [HttpGet]
        public async Task<IActionResult> JobStats()
        {
            var response = await _elasticClient.SearchAsync<Models.Job>(s => s
                .Size(0) // Don't return documents
                .Aggregations(a => a
                    .Terms("titles", t => t.Field(f => f.Title.Suffix("keyword")).Size(10))
                    .Terms("companies", c => c.Field(f => f.Company.Suffix("keyword")).Size(10))
                )
            );

            var titleBuckets = response.Aggregations.Terms("titles").Buckets;
            var companyBuckets = response.Aggregations.Terms("companies").Buckets;

            var stats = new
            {
                Titles = titleBuckets.Select(b => new { Name = b.Key, Count = b.DocCount }),
                Companies = companyBuckets.Select(b => new { Name = b.Key, Count = b.DocCount })
            };

            return Json(stats);
        }

      



    }
}
