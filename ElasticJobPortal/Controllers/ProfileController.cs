using ElasticJobPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

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
        public async Task<IActionResult> UploadResume(IFormFile resume)
        {
            if (resume != null && resume.Length > 0)
            {
                // Save the resume to /wwwroot/resumes
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "resumes");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + resume.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await resume.CopyToAsync(fileStream);
                }

                var relativePath = "/resumes/" + uniqueFileName;

                // 🔑 Get current user
                var user = await _usermanager.GetUserAsync(User);

               
               
            }

            TempData["ErrorMessage"] = "Invalid file.";
            return RedirectToAction("Index");
        }



    }
}
