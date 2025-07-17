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

        // show categories
        public IActionResult ListCategories()
        {
            var categories = _context.QuizCategories.ToList();
            return View(categories);
        }

        //delete category
        [HttpGet]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.QuizCategories.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
                return NotFound();
            return View(category);
        }

        [HttpPost, ActionName("DeleteCategory")]
        public async Task<IActionResult> DeleteCategoryConfirmed(int id)
        {
            var category = await _context.QuizCategories
                .Include(c => c.Questions)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return NotFound();

            if (category.Questions != null && category.Questions.Any())
            {
                TempData["Error"] = "Cannot delete category that has questions.";
                return RedirectToAction("ListCategories");
            }

            _context.QuizCategories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Category deleted successfully.";
            return RedirectToAction("ListCategories");
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

        //show questions
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

        //Edit Question
        [HttpGet]
        public async Task<IActionResult> EditQuestion(int id)
        {
            var question = await _context.QuizQuestions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
                return NotFound();

            ViewBag.CategoryId = question.CategoryId;
            return View(question);
        }

        [HttpPost]
        public async Task<IActionResult> EditQuestion(QuizQuestion question)
        {
            var existingQuestion = await _context.QuizQuestions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == question.Id);

            if (existingQuestion == null)
                return NotFound();

            existingQuestion.QuestionText = question.QuestionText;
            existingQuestion.CategoryId = question.CategoryId;

            for (int i = 0; i < question.Answers.Count; i++)
            {
                var answer = question.Answers[i];
                var existingAnswer = existingQuestion.Answers[i];

                existingAnswer.AnswerText = answer.AnswerText;
                existingAnswer.IsCorrect = answer.IsCorrect;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("ListQuestions", new { categoryId = question.CategoryId });

        }

        // Delete Question
        [HttpGet]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var question = await _context.QuizQuestions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);
            if (question == null)
                return NotFound();
            return View(question);
        }

        [HttpPost, ActionName("DeleteQuestion")]
        public async Task<IActionResult> DeleteQuestionConfirmed(int id)
        {
            var question = await _context.QuizQuestions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);
            if (question != null)
            { 
                _context.QuizQuestions.Remove(question);
                _context.QuizAnswers.RemoveRange(question.Answers);
                await _context.SaveChangesAsync();
            }
               
            return RedirectToAction("ListQuestions");

        }


    }
}
