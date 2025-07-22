using ElasticJobPortal.Models;
using ElasticJobPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        //show all subscription plans
        public async Task<IActionResult> PlanList()
        {
            var plans = await _context.SubscriptionPlans.ToListAsync();
            return View(plans);
        }

        //create a new subscription plan
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

        //edit a subscription plan
        public async Task<IActionResult> Edit(int id)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(id);
            if (plan == null) return NotFound();
            return View(plan);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(SubscriptionPlan plan)
        {
            if (ModelState.IsValid)
            {
                _context.SubscriptionPlans.Update(plan);
                await _context.SaveChangesAsync();
                return RedirectToAction("PlanList");
            }
            return View(plan);
        }

        //delete a subscription plan
        public async Task<IActionResult> Delete(int id)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(id);
            if (plan == null) return NotFound();
            return View(plan);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(id);
            if (plan != null)
            {
                _context.SubscriptionPlans.Remove(plan);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("PlanList");
        }


    }
}
