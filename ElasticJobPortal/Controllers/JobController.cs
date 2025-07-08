using ElasticJobPortal.Elastic;
using ElasticJobPortal.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ElasticJobPortal.Controllers
{
    public class JobController : Controller
    {
        private readonly JobSearchService _searchService = new();
        private readonly JobIndexerService _indexer = new();

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
                return View(new List<Job>());

            var results = _searchService.SearchJobs(keyword);
            return View(results);
        }

        // Optional: Show all jobs (dummy)
        public IActionResult Index()
        {
            return RedirectToAction("Search");
        }
    }
}
