using ElasticJobPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElasticJobPortal.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace ElasticJobPortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var logs = await _context.JobApplications
                 .Include(j => j.Job)
                 .Include(j => j.User)
                .OrderByDescending(j => j.AppliedOn)
                .ToListAsync();

            return View(logs);

        }

        public async Task<IActionResult> ExportCsv() 
        {
            var applications  = await _context.JobApplications
                .Include(j => j.Job)
                .Include(j => j.User)
                .OrderByDescending(j => j.AppliedOn)
                .ToListAsync();

            var csv = new StringBuilder();
            csv.AppendLine("Company Name,Job Title,UserName,Email,Applied On,Status");

            foreach (var app in applications)
            {
                csv.AppendLine($"\"{app.Job?.Company}\",\"{app.Job?.Title}\",\"{app.User?.FullName}\",\"{app.User?.Email}\",\"{app.AppliedOn}\",\"{app.Status}\"");
            }

            return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "audit_logs.csv");
        }
    }
}
