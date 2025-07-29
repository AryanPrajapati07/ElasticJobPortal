
using ElasticJobPortal.Helpers;
using ElasticJobPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ElasticJobPortal.Controllers
{
    [Authorize(Roles = "JobSeeker")]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _usermanager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _usermanager = userManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _usermanager.GetUserAsync(User);

            var vm = new ProfileViewModel
            {
                FullName = user.FullName,
                Bio = user.Bio,
                Location = user.Location,
                ResumePath = user.ResumePath,
                Email = user.Email,
                ResumeKeywords = user.ResumeKeywords,
                Applications = await _context.JobApplications
                    .Include(a => a.Job)
                    .Where(a => a.UserId == user.Id)
                    .ToListAsync(),
                SavedJobs = await _context.SavedJobs
                    .Include(s => s.Job)
                    .Where(s => s.UserId == user.Id)
                    .ToListAsync()
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Index(ProfileViewModel model)
        {
            var user = await _usermanager.GetUserAsync(User);
            if (model.ResumeFile != null) 
            { 
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "resumes");
                Directory.CreateDirectory(uploads);
                var fileName = $"{Guid.NewGuid()}_{model.ResumeFile.FileName}";
                var filePath = Path.Combine(uploads, fileName);

                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    await model.ResumeFile.CopyToAsync(fs);
                }

                user.ResumePath = $"/resumes/{fileName}";

            }

            user.FullName = model.FullName;
            user.Bio = model.Bio;
            user.Location = model.Location;
            
            await _usermanager.UpdateAsync(user);

            TempData["SuccessMessage"] = "Profile updated successfully!";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> PublicProfile(string id)
        {
            var user = await _usermanager.FindByIdAsync(id);
            if (user == null)             {
                return NotFound();
            }
            return View(user);

        }

        [Authorize(Roles ="JobSeeker")]
        public async Task<IActionResult> Dashboard() 
        {
            var user = await _usermanager.GetUserAsync(User);
            var applications = await _context.JobApplications
                .Where(a => a.UserId == user.Id)
                .CountAsync();
            var saved = await _context.SavedJobs
                .Where(s => s.UserId == user.Id)
                .CountAsync();

            ViewBag.ApplicationsCount = applications;
            ViewBag.SavedJobsCount = saved;
            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> UploadResume(IFormFile resume)
        {
            var user = await _usermanager.GetUserAsync(User);

            if (resume == null || resume.Length == 0)
            {
                TempData["Message"] = "Invalid file.";
                return RedirectToAction("Profile");
            }

            // Save the resume
            var uploads = Path.Combine(_webHostEnvironment.WebRootPath, "resumes");
            if (!Directory.Exists(uploads))
                Directory.CreateDirectory(uploads);

            var extension = Path.GetExtension(resume.FileName);
            var fileName = user.Id + extension;
            var fullPath = Path.Combine(uploads, fileName);
            var relativePath = "/resumes/" + fileName;

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await resume.CopyToAsync(stream);
            }

            // Parse Keywords
            var keywordsList = ResumeParser.ExtractKeywordsFromPdf(relativePath);
            var keywordString = string.Join(",", keywordsList);

            // Debug Output
            Console.WriteLine("Parsed Keywords: " + keywordString);

            // Assign to user
            user.ResumePath = relativePath;
            user.ResumeKeywords = keywordString;

            // Save using UserManager (NOT _context)
            var result = await _usermanager.UpdateAsync(user);

            // Debug output
            Console.WriteLine("UserManager Update Result: " + result.Succeeded);

            if (result.Succeeded)
                TempData["Message"] = "Resume uploaded and keywords saved.";
            else
                TempData["Message"] = "Upload failed: " + string.Join(", ", result.Errors.Select(e => e.Description));

            return RedirectToAction("Profile");
        }




    }
}
