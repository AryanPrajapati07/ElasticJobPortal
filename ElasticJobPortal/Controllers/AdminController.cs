using ElasticJobPortal.Models;
using ElasticJobPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElasticJobPortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var totalJobs = await _context.Jobs.CountAsync();
            var totalApplications = await _context.JobApplications.CountAsync();

            var statusBreakdown = await _context.JobApplications
                .GroupBy(a => a.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            ViewBag.TotalJobs = totalJobs;
            ViewBag.TotalApplications = totalApplications;
            ViewBag.StatusBreakdown = statusBreakdown;

            return View();
        }
    }
}
