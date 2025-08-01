﻿using ElasticJobPortal.Helpers;
using ElasticJobPortal.Models;
using ElasticJobPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using X.PagedList;
using X.PagedList.Extensions;

namespace ElasticJobPortal.Controllers
{
    [Authorize]

    public class JobController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _usermanager;
        private readonly IEmailService _emailService;
        private readonly RecommendationService _recommendationService;


        public JobController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailService emailService,RecommendationService recommendationService)
        {

            _context = context;
            _usermanager = userManager;
            _emailService = emailService;
            _recommendationService = recommendationService;
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
        public async Task<IActionResult> AllApplications(string status = "all")
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

            // Check if already applied
            var alreadyApplied = await _context.JobApplications
                .AnyAsync(a => a.JobId == jobId && a.UserId == userId);

            if (alreadyApplied)
            {
                TempData["ErrorMessage"] = "You have already applied for this job.";
                return RedirectToAction("AvailableJobs");
            }

            // Upload resume
            string resumePath = null;
            if (resumeFile != null && resumeFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/resumes");
                Directory.CreateDirectory(uploadsFolder); // Ensure folder exists

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(resumeFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await resumeFile.CopyToAsync(fileStream);
                }

                resumePath = $"/resumes/{uniqueFileName}";
            }

            // Save to DB
            var application = new JobApplication
            {
                JobId = jobId,
                UserId = userId,
                AppliedOn = DateTime.Now,
                ResumePath = resumePath
            };

            _context.JobApplications.Add(application);
            await _context.SaveChangesAsync();

            // Send Email to Admin


            // Fetch Job and User Details for Email
            var job = await _context.Jobs.FindAsync(jobId);
            var user = await _usermanager.GetUserAsync(User); // get full ApplicationUser object

            // Send Email to User
            if (user != null && job != null)
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Job Application Submitted",
                    $"You have successfully applied for <b>{job.Title}</b>. Thank you for using our portal!"
                );
            }

            TempData["SuccessMessage"] = "Application submitted successfully!";
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



        // Update the problematic line in the AvailableJobs method
        [Authorize(Roles = "JobSeeker")]

        public async Task<IActionResult> AvailableJobs(string keyword, int page = 1)
        {
            var userId = _usermanager.GetUserId(User);

            // Get applied job IDs
            var appliedJobIds = await _context.JobApplications
                .Where(a => a.UserId == userId)
                .Select(a => a.JobId)
                .ToListAsync();

            // No sorting applied here
            var jobsQuery = _context.Jobs
                .Where(j => !j.IsExpired ||
                    string.IsNullOrEmpty(keyword) ||
                    j.Title.Contains(keyword) ||
                    j.Description.Contains(keyword)
                );

            int pageSize = 6;

            var pagedJobs = jobsQuery.AsEnumerable().ToPagedList(page, pageSize);

            ViewBag.AppliedJobs = appliedJobIds;
            ViewBag.Keyword = keyword;

            return View(pagedJobs);
        }



        [HttpPost]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> SaveJob(int jobId)
        {
            var userId = _usermanager.GetUserId(User);
            // Check if already saved
            var alreadySaved = await _context.SavedJobs
                .AnyAsync(s => s.JobId == jobId && s.UserId == userId);
            if (!alreadySaved)
            {
                _context.SavedJobs.Add(new SavedJob
                {
                    JobId = jobId,
                    UserId = userId,
                    SavedOn = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("AvailableJobs");
        }

        [HttpPost]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> UnsaveJob(int jobId)
        {
            var userId = _usermanager.GetUserId(User);
            var savedJob = await _context.SavedJobs
                .FirstOrDefaultAsync(s => s.JobId == jobId && s.UserId == userId);
            if (savedJob != null)
            {
                _context.SavedJobs.Remove(savedJob);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("AvailableJobs");
        }

        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> SavedJobs(int page = 1)
        {
            var userId = _usermanager.GetUserId(User);
            var saved = await _context.SavedJobs
                .Include(s => s.Job)
                .Where(s => s.UserId == userId)
                .ToListAsync();
            return View(saved);

        }

        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> RecommendedJobs()
        {
            var user = await _usermanager.GetUserAsync(User);
            ViewBag.ResumeKeywords = user.ResumeKeywords;

            if (string.IsNullOrEmpty(user.ResumePath))
            {
                ViewBag.Message = "Please upload a resume to get job recommendations.";
                return View(new List<Job>());
            }

            var resumeKeywords = ResumeParser.ExtractKeywordsFromPdf(user.ResumePath); // ✅ returns List<string>

            var allJobs = _context.Jobs.ToList();

            var matchedJobs = allJobs.Where(job =>
            {
                var jobTags = job.Tags?.ToLower().Split(',').Select(t => t.Trim()) ?? Enumerable.Empty<string>();
                return jobTags.Intersect(resumeKeywords).Any();
            }).ToList();

            return View(matchedJobs);
        }


    }
}
