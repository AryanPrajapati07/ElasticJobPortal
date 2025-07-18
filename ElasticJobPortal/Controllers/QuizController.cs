using ElasticJobPortal.Models;
using ElasticJobPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;



namespace ElasticJobPortal.Controllers
{
    [Authorize(Roles ="JobSeeker")]
    public class QuizController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _usermanager;

        public QuizController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _usermanager = userManager;
        }

        public async Task<IActionResult> Index() 
        {
            var categories = await _context.QuizCategories.ToListAsync();
            return View(categories);
        }

        public async Task<IActionResult> Start(int categoryId)
        {
            var questions = await _context.QuizQuestions
                .Where(q => q.CategoryId == categoryId)
                .Include(q => q.Answers)
                .Take(25) // 5 questions per quiz
                .ToListAsync();

            ViewBag.CategoryId = categoryId;
            ViewBag.TimeLimitInMinutes = 10;
            return View(questions);
        }

        [HttpPost]
        public async Task<IActionResult> Submit(int categoryId, Dictionary<int, int> answers)
        {
            int correct = 0;
            foreach (var entry in answers)
            {
                var answer = await _context.QuizAnswers.FindAsync(entry.Value);
                if (answer != null && answer.IsCorrect)
                {
                    correct++;
                }
            }

            var userId = _usermanager.GetUserId(User);
            var result = new QuizResult
            {
                UserId = userId,
                CategoryId = categoryId,
                Score = correct,
                TotalQuestions = answers.Count,
                TakenOn = DateTime.UtcNow
            };

            _context.QuizResults.Add(result);
            await _context.SaveChangesAsync();


            return RedirectToAction("Result", new { id = result.Id});

        }

        public async Task<IActionResult> Result(int id)
        {
            var result = await _context.QuizResults
                .Include(r => r.Category)
                .FirstOrDefaultAsync(r => r.Id == id);
            if (result == null) return NotFound();
            return View(result);
        }

        public IActionResult DownloadCertificate(int quizId)
        {
            var model = new CertificateViewModel
            {
                Fullname = User.Identity.Name,
                QuizTitle = "Quiz Title", // You can fetch the actual title from the database if needed
                Score = "Score", // Fetch the score from the database
                CompletionDate = DateTime.UtcNow
            };
            ReportDocument report = new ReportDocument();
            report.Load(Path.Combine(Directory.GetCurrentDirectory(), "Reports", "CertificateReport.rpt"));
            report.SetDatasource(new List<CertificateViewModel> { model });
            Stream stream = report.ExportToStream("PortableDocFormat"); 
            stream.Seek(0,SeekOrigin.Begin);
            return File(stream,"application/pdf","QuizCertificate.pdf");

        }
    }

    internal class ReportDocument
    {
        public ReportDocument()
        {
        }

        internal Stream ExportToStream(object portableDocFormat)
        {
            throw new NotImplementedException();
        }

        internal void Load(string v)
        {
            throw new NotImplementedException();
        }

        internal void SetDatasource(List<CertificateViewModel> certificateViewModels)
        {
            throw new NotImplementedException();
        }
    }
}

