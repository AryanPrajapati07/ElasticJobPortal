
using ElasticJobPortal.Services;
using Microsoft.AspNetCore.Authorization;
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

        public ProfileController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _usermanager = userManager;
            _context = context;
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
        public async Task<IActionResult> UploadResume(IFormFile resumeFile)
        {

            var user = await _usermanager.GetUserAsync(User);
            if (resumeFile != null && resumeFile.Length>0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "resumes");
                Directory.CreateDirectory(uploads);
                var fileName = $"{Guid.NewGuid()}_{resumeFile.FileName}";
                var filePath = Path.Combine(uploads, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await resumeFile.CopyToAsync(stream);

                user.ResumePath = $"/resumes/{fileName}";
                await _usermanager.UpdateAsync(user);
            }
            return RedirectToAction("Dashboard");

        }


    }
}
