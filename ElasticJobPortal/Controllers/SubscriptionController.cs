using ElasticJobPortal.Models;
using ElasticJobPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElasticJobPortal.Controllers
{
    [Authorize(Roles = "JobSeeker")]
    public class SubscriptionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SubscriptionController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Plans()
        {
            var plans = _context.SubscriptionPlans.ToList();
            return View(plans);
        }

        [HttpPost]
        public async Task<IActionResult> Subscribe(int planId)
        {
            var userId = _userManager.GetUserId(User);
            var plan = await _context.SubscriptionPlans.FindAsync(planId);
            if (plan == null)
            {
                return NotFound();
            }
            var subscription = new JobSeekerSubscription
            {
                UserId = userId,
                PlanId = plan.Id,
                SubscribedOn = DateTime.UtcNow,
                ExpiryDate = plan.Duration.ToLower() == "lifetime" ? (DateTime?)null : DateTime.UtcNow.AddMonths(1)
            };
            _context.JobSeekerSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();
            return RedirectToAction("MySubscription");
        }

        public async Task<IActionResult> MySubscription()
        {
            var userId = _userManager.GetUserId(User);
            var subscription = await _context.JobSeekerSubscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.UserId == userId);
           
            return View(subscription);

        }

        public async Task<IActionResult> Buy(int id)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(id);
            if (plan == null) return NotFound();

            ViewBag.PlanId = plan.Id;
            ViewBag.PlanName = plan.Name;
            ViewBag.Price = Convert.ToDecimal(plan.Price);
            ViewBag.Duration = plan.Duration;

            return View(); // Buy.cshtml
        }

        public async Task<IActionResult> MyPayments()
        {
            var userId = _userManager.GetUserId(User);
            var payments = await _context.PaymentDetails
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
            return View(payments);
        }



    }
}
