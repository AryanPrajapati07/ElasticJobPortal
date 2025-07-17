using ElasticJobPortal.Models;
using ElasticJobPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElasticJobPortal.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminQuizController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminQuizController(ApplicationDbContext context)
        {
            _context = context;
        }

        //create category
        [HttpGet]
        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(QuizCategory category)
        {
            var exists = await _context.QuizCategories
                .AnyAsync(q => q.Name.ToLower() == category.Name.ToLower());

            if (exists)
            {
                ModelState.AddModelError("Name", "Category already exists.");
                return View(category);
            }


            _context.QuizCategories.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction("ListCategories");
        }

        public IActionResult ListCategories()
        {
            var categories = _context.QuizCategories.ToList();
            return View(categories);
        }


        //Add Question
        [HttpGet]
        public IActionResult CreateQuestion(int categoryId)
        {
            ViewBag.CategoryId = categoryId;
            return View(new QuizQuestion { Answers = new List<QuizAnswer> { new(), new(), new(), new() } });
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuestion(QuizQuestion question)
        {
            // Ensure this isn't null
            if (question.Answers == null || !question.Answers.Any())
            {
                ModelState.AddModelError("", "At least one answer is required.");
                ViewBag.CategoryId = question.CategoryId;
                return View(question);
            }

           

            // Save related answers
            foreach (var answer in question.Answers)
            {
                answer.Question = question;

            }
            _context.QuizQuestions.Add(question);
            await _context.SaveChangesAsync();

            return RedirectToAction("ListCategories");
        }

        public IActionResult ListQuestions(int? categoryId)
        {
            var categories = _context.QuizCategories.ToList();
            ViewBag.Categories = categories;

            var questions = _context.QuizQuestions
                .Include(q => q.Category)
                .Include(q => q.Answers)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                questions = questions.Where(q => q.CategoryId == categoryId);
            }

            return View(questions.ToList());
        }


    }
}
