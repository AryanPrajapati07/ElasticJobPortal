
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

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AllApplications()
        {
            var applications = await _context.JobApplications
                .Include(a => a.Job)
                .Include(a => a.User)
                .ToListAsync();

            return View(applications);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int applicationId, string status)
        {
            var application = await _context.JobApplications.FindAsync(applicationId);
            if (application == null) return NotFound();

            application.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToAction("AllApplications");
        }




        public IActionResult List()
        {
            var jobs = _context.Jobs.ToList();
            return View(jobs);
        }


        [HttpPost]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> Apply(int jobId, IFormFile resumeFile)
        {
            var userId = _usermanager.GetUserId(User);

            var alreadyApplied = await _context.JobApplications
                .AnyAsync(a => a.JobId == jobId && a.UserId == userId);

            if (alreadyApplied)
            {
                TempData["ErrorMessage"] = "You already applied for this job.";
                return RedirectToAction("AvailableJobs");
            }

            // ✅ Save resume to wwwroot/resumes
            string resumePath = null;
            if (resumeFile != null && resumeFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/resumes");
                Directory.CreateDirectory(uploadsFolder); // Ensure folder exists
                var uniqueFileName = $"{Guid.NewGuid()}_{resumeFile.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await resumeFile.CopyToAsync(fileStream);
                }

                resumePath = $"/resumes/{uniqueFileName}";
            }

            var application = new JobApplication
            {
                JobId = jobId,
                UserId = userId,
                AppliedOn = DateTime.Now,
                ResumePath = resumePath
            };

            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Applied successfully!";
            return RedirectToAction("AvailableJobs");
        }




        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> MyApplications()
        {
            var userId = _usermanager.GetUserId(User);
            var applications = await _context.JobApplications
                .Include(j => j.Job)
                .Where(j => j.UserId == userId)
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
        public async Task<IActionResult> AvailableJobs()
        {
            var jobs = await _context.Jobs.ToListAsync();
            var userId = _usermanager.GetUserId(User);

            var appliedJobIds = await _context.JobApplications
                .Where(a => a.UserId == userId)
                .Select(a => a.JobId)
                .ToListAsync();

            ViewBag.AppliedJobs = appliedJobIds;

            return View(jobs);
        }







    }
}
