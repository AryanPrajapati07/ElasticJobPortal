
using ElasticJobPortal.Models;
using ElasticJobPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace ElasticJobPortal.Controllers
{
    [Authorize]

    public class JobController : Controller
    {
        
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _usermanager;


        public JobController(ApplicationDbContext context,UserManager<ApplicationUser> userManager)
        {
           
            _context = context;
            _usermanager = userManager;
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

            return RedirectToAction("List");
        }


        public IActionResult List()
        {
            var jobs = _context.Jobs.ToList();
            return View(jobs);
        }


        [HttpPost]
        [Authorize(Roles = "JobSeeker")]
        public IActionResult Apply(int jobId)
        {
            // Placeholder - You can store applications later
            TempData["SuccessMessage"] = "Application submitted successfully.";
            return RedirectToAction("AvailableJobs");
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


        // Search jobs
        public IActionResult Search(string keyword)
        {
            var jobs = _context.Jobs
                .Where(j => j.Title.Contains(keyword) || j.Description.Contains(keyword))
                .ToList();
            return View(jobs);
        }


        // Optional: Show all jobs (dummy)
        public IActionResult Index()
        {
            return RedirectToAction("Search");
        }

        [Authorize(Roles = "JobSeeker")]
        public IActionResult AvailableJobs()
        {
            var jobs = _context.Jobs.ToList();
            return View(jobs);
        }






    }
}
