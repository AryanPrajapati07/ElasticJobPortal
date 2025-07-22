using ElasticJobPortal.Models;
using ElasticJobPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElasticJobPortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminSubscriptionController : Controller
    {
       private readonly ApplicationDbContext _context;
        public AdminSubscriptionController(ApplicationDbContext context)
        {
            _context = context;
        }
       
        public IActionResult PlanList()
        {
            var plans = _context.SubscriptionPlans.ToList();
            return View(plans);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubscriptionPlan plan)
        {
            _context.SubscriptionPlans.Add(plan);
            await _context.SaveChangesAsync();
            return RedirectToAction("Create");
        }

    }
}
